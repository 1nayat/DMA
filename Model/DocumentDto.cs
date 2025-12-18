namespace DMA.Model
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? UploadedBy { get; set; } // optional, only for admin view
        public string Status { get; set; } = "Uploaded";
    }

}
