using Student_Management_System.Dtos.Admin;

namespace Student_Management_System.Services.Interfaces;

public interface IDatabaseMaintenanceService
{
    public const string RequiredConfirmation = "XOA TOAN BO DU LIEU";

    Task<ResetDatabaseResponse?> ResetAsync(
        string confirmation,
        CancellationToken cancellationToken = default);
}
