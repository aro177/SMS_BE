namespace Student_Management_System.Config
{
    public class SupabaseOptions
    {
        public string Url { get; set; } = string.Empty;

        public string ApiSecretKey { get; set; } = string.Empty;

        public int UploadUrlExpireTimeInSeconds { get; set; } = 600;

        public int ViewUrlExpireTimeInSeconds { get; set; } = 900;
    }
}
