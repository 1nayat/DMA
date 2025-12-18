using System.Security.Claims;
using DMA.Data;
using DMA.Entities;
using DMA.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DMA.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto dto, ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim?.Value!);



            var originalName = Path.GetFileNameWithoutExtension(dto.File.FileName);
            var extension = Path.GetExtension(dto.File.FileName);
            var fileName = $"{originalName}_{Guid.NewGuid()}{extension}";

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(fileStream);
            }

            var document = new Document
            {
                FileName = fileName,
                FilePath = filePath,
                Description = dto.Description,
                UploadedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
            var dbUser = await _context.Users.FindAsync(userId);

            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FilePath = document.FilePath,
                Description = document.Description,
                UploadedAt = document.UploadedAt,
                //	Null-coalescing operator ??:- if the left side is null use the value on the right side
                //Null-conditional operator ? :-If dbUser is null, it returns null instead of throwing an exception.
                ////Unknown will be result in both cases
                UploadedBy = dbUser ?.UserName ?? "Unknown",
                Status = document.Status
            };
        }

        public async Task<IEnumerable<DocumentDto>> GetMyDocumentsAsync(ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            return await _context.Documents
                .Where(d => d.UserId == userId)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    Description = d.Description,
                    UploadedAt = d.UploadedAt,
                    UploadedBy = d.User.UserName,
                    Status = d.Status

                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    Description = d.Description,
                    UploadedAt = d.UploadedAt,
                    UploadedBy = d.User.UserName,
                    Status = d.Status
                })
                .ToListAsync();
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int documentId, ClaimsPrincipal user)
        {
            var doc = await _context.Documents.FindAsync(documentId);
            if (doc == null) return null;

            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            if (doc.UserId != userId && role != "Admin")
                return null;

            return new DocumentDto
            {
                Id = doc.Id,
                FileName = doc.FileName,
                FilePath = doc.FilePath,
                Description = doc.Description,
                UploadedAt = doc.UploadedAt,
                UploadedBy = doc.User.UserName,
                Status = doc.Status
            };
        }

        public async Task<bool> DeleteDocumentAsync(int documentId, ClaimsPrincipal user)
        {
            var doc = await _context.Documents.FindAsync(documentId);
            if (doc == null) return false;

            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            if (doc.UserId != userId && role != "Admin")
                return false;

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            var fullPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return true;
        }

        public async Task<(Stream FileStream, string FileName, string ContentType)?> DownloadDocumentAsync(int documentId, ClaimsPrincipal user)
        {
            var doc = await _context.Documents.FindAsync(documentId);
            if (doc == null) return null;

            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

           
            if (doc.UserId != userId && role != "Admin")
                return null;

            var fullPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));

            if (!File.Exists(fullPath))
                return null;

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(fullPath);
            var contentType = "application/octet-stream"; 

            return (fileStream, fileName, contentType);
        }



        public async Task<PaginatedResult<DocumentDto>> GetMyDocumentsAsync(
     ClaimsPrincipal user,
     PaginationParams paginationParams,
     DocumentQueryParams queryParams)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Document> query;

            if (role == "Admin")
            {
                query = _context.Documents.Include(d => d.User).AsQueryable();
            }
            else
            {
                query = _context.Documents.Where(d => d.UserId == userId);
            }

            //if (queryParams.StartDate.HasValue)
            //{
            //    query = query.Where(d => d.UploadedAt >= queryParams.StartDate.Value);
            //}

            //if (queryParams.EndDate.HasValue)
            //{
            //    query = query.Where(d => d.UploadedAt <= queryParams.EndDate.Value);
            //}
            if (queryParams.StartDate.HasValue && queryParams.EndDate.HasValue)
            {
                var start = queryParams.StartDate.Value.Date;
                var end = queryParams.EndDate.Value.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

                query = query.Where(d => d.UploadedAt >= start && d.UploadedAt <= end);
            }


            if (!string.IsNullOrWhiteSpace(queryParams.UploadedBy))
            {
                query = query.Where(d => d.User.UserName.ToLower().Contains(queryParams.UploadedBy.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.UploadedAt)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    UploadedAt = d.UploadedAt,
                    UploadedBy = d.User.UserName ,
                    Description = d.Description,
                    Status = d.Status
                })
                .ToListAsync();

            return new PaginatedResult<DocumentDto>
            {
                Items = items,
                CurrentPage = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> UpdateDocumentStatusAsync(int documentId, string newStatus)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            document.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAndDocumentsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            var documents = _context.Documents.Where(d => d.UserId == userId);

            _context.Documents.RemoveRange(documents);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

    }
}
