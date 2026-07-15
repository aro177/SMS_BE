using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.ClassRegistrations;
using Student_Management_System.Models.Enum;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/class-registrations")]
public class ClassRegistrationsController : ControllerBase
{
    private readonly IClassRegistrationService _registrations;

    public ClassRegistrationsController(IClassRegistrationService registrations)
    {
        _registrations = registrations;
    }

    [HttpGet]
    public async Task<IActionResult> GetRegistrations(
        [FromQuery] EnrollmentStatus? status,
        [FromQuery] PaginationQuery pagination)
    {
        return Ok(await _registrations.GetPagedAsync(status, pagination));
    }

    [HttpPost]
    public async Task<IActionResult> RegisterClass(ClassRegistrationRequest request)
    {
        var result = await _registrations.RegisterAsync(request);
        return result is null ? NotFound("Classroom not found.") : Created("/api/class-registrations", result);
    }

    [HttpPut("{id:long}/approve")]
    public async Task<IActionResult> ApproveRegistration(long id)
    {
        return await _registrations.ApproveAsync(id) ? NoContent() : NotFound();
    }

    [HttpPut("{id:long}/reject")]
    public async Task<IActionResult> RejectRegistration(long id)
    {
        return await _registrations.RejectAsync(id) ? NoContent() : NotFound();
    }
}
