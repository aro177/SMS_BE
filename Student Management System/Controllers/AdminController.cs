using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Dtos.Admin;
using Student_Management_System.Models.Enum;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.ADMIN))]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IDatabaseMaintenanceService _databaseMaintenance;

    public AdminController(IDatabaseMaintenanceService databaseMaintenance)
    {
        _databaseMaintenance = databaseMaintenance;
    }

    [HttpPost("database/reset")]
    public async Task<IActionResult> ResetDatabase(
        ResetDatabaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _databaseMaintenance.ResetAsync(
            request.Confirmation,
            cancellationToken);

        return result is null
            ? BadRequest(new
            {
                message = $"Confirmation must exactly match '{IDatabaseMaintenanceService.RequiredConfirmation}'."
            })
            : Ok(result);
    }
}
