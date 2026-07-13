using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Dtos.ClassRegistrations;
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

    [HttpPost]
    public async Task<IActionResult> RegisterClass(ClassRegistrationRequest request)
    {
        var result = await _registrations.RegisterAsync(request);
        return result is null ? NotFound("Classroom not found.") : Created("/api/class-registrations", result);
    }
}
