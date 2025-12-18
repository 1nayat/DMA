using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMA.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = null!;

        [Required]
        public string FilePath { get; set; } = null!;
        
        public string ? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key(Navigation prop name )
        [ForeignKey("User")]
        public int UserId { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
        public string Status { get; set; } = "Uploaded";
    }
}

