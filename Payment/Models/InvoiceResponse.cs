namespace RazorpayApi.Models
{
    public class InvoiceResponse
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public byte[]? PdfBytes { get; set; }
    }
}
