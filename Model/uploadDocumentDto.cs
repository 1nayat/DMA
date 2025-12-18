using System.ComponentModel.DataAnnotations;

namespace DMA.Model
{
    public class UploadDocumentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        public string? Description { get; set; }
    }

}
