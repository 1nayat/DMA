using DMA.Entities;
using DMA.Model;
using DMA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DMA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

      
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto dto)
        {
            var result = await _documentService.UploadDocumentAsync(dto, User);
            return Ok(result);
        }

        [HttpGet("myDocument")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var result = await _documentService.GetMyDocumentsAsync(User);
            return Ok(result);
        }

        [HttpGet("allDocuments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDocuments()
        {
            var result = await _documentService.GetAllDocumentsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var result = await _documentService.GetDocumentByIdAsync(id, User);
            if (result == null)
                return Forbid(); // Either not found or access denied

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var success = await _documentService.DeleteDocumentAsync(id, User);
            if (!success)
                return Forbid(); // Either not found or not authorized

            return NoContent();
        }
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var result = await _documentService.DownloadDocumentAsync(id, User);

            if (result is { } tuple)
            {
                var fileStream = tuple.FileStream;
                var fileName = tuple.FileName;
                var contentType = tuple.ContentType;

                return File(fileStream, contentType, fileName);
            }

            return Forbid(); 
        }
        [HttpGet("Pagination/Filtering")]
        public async Task<IActionResult> GetMyDocuments([FromQuery] PaginationParams paginationParams, [FromQuery] DocumentQueryParams queryParams)
        {
            var result = await _documentService.GetMyDocumentsAsync(User, paginationParams, queryParams);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDocumentStatus(int id, [FromBody] string newStatus)
        {
            var result = await _documentService.UpdateDocumentStatusAsync(id, newStatus);
            if (!result)
                return NotFound(new { message = "Document not found." });

            return Ok(new { message = "Status updated successfully." });
        }
        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("API is connected!");
        }

        [HttpDelete("delete-user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserAndDocuments(int userId)
        {
            var result = await _documentService.DeleteUserAndDocumentsAsync(userId);
            if (!result)
                return NotFound("User not found or already deleted.");

            return Ok("User and their documents deleted successfully.");
        }

        [HttpGet("Get-users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _documentService.GetAllUsersAsync();
            return Ok(users);
        }

    }
}
