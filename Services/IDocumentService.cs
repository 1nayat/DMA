using DMA.Entities;
using DMA.Model;
using System.Security.Claims;

namespace DMA.Services
{
    public interface IDocumentService
    {
        Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto dto, ClaimsPrincipal user);

        Task<IEnumerable<DocumentDto>> GetMyDocumentsAsync(ClaimsPrincipal user);

        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();

        Task<DocumentDto?> GetDocumentByIdAsync(int documentId, ClaimsPrincipal user);

        Task<bool> DeleteDocumentAsync(int documentId, ClaimsPrincipal user);
        Task<(Stream FileStream, string FileName, string ContentType)?> DownloadDocumentAsync(int documentId, ClaimsPrincipal user);

        Task<PaginatedResult<DocumentDto>> GetMyDocumentsAsync(ClaimsPrincipal user, PaginationParams paginationParams,DocumentQueryParams queryParams);

        Task<bool> UpdateDocumentStatusAsync(int documentId, string newStatus);
        Task<bool> DeleteUserAndDocumentsAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();


    }
}
