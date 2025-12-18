namespace DMA.Entities
{
    public class DocumentQueryParams
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? UploadedBy { get; set; } // make it nullable to avoid non-nullable constructor error
    }


}
