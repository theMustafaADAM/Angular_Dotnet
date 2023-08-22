namespace ModelService
{
    public class SmtpOptions
    {
        public string? FromFullName { get; set; }
        public string? FromEmail { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SmtpHost { get; set; }
        public bool IsDefault { get; set; }
        public bool SmtpSSl { get; set; }
        public int SmtpPort { get; set; }
    }
}