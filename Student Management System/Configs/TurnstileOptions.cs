namespace Student_Management_System.Configs;

public class TurnstileOptions
{
    public string SecretKey { get; set; } = string.Empty;

    public string ExpectedHostname { get; set; } = string.Empty;
}
