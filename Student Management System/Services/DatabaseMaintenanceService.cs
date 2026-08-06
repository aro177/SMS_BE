using Microsoft.EntityFrameworkCore;
using Student_Management_System.Dtos.Admin;
using Student_Management_System.Models;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class DatabaseMaintenanceService : IDatabaseMaintenanceService
{
    private const int TruncatedTableCount = 7;

    private const string TruncateApplicationTablesSql = """
        TRUNCATE TABLE
            "public"."attendances",
            "public"."enrollments",
            "public"."lessons",
            "public"."classrooms",
            "public"."students",
            "public"."parents",
            "public"."teachers"
        RESTART IDENTITY CASCADE;
        """;

    private readonly AppDbContext _context;

    public DatabaseMaintenanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResetDatabaseResponse?> ResetAsync(
        string confirmation,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(
                confirmation,
                IDatabaseMaintenanceService.RequiredConfirmation,
                StringComparison.Ordinal))
        {
            return null;
        }

        var executionStrategy = _context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            await _context.Database.ExecuteSqlRawAsync(
                TruncateApplicationTablesSql,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });

        return new ResetDatabaseResponse(TruncatedTableCount);
    }
}
