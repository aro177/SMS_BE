namespace Student_Management_System.Services.Interfaces;

public interface ITurnstileVerificationService
{
    Task<bool> VerifyStudentSearchAsync(
        string token,
        string? remoteIp,
        CancellationToken cancellationToken = default);
}
