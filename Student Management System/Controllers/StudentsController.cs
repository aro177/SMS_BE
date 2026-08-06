using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Students;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _students;
    private readonly ITurnstileVerificationService _turnstile;

    public StudentsController(
        IStudentService students,
        ITurnstileVerificationService turnstile)
    {
        _students = students;
        _turnstile = turnstile;
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] PaginationQuery pagination)
    {
        return Ok(await _students.GetPagedAsync(pagination));
    }

    [HttpPost("search")]
    [EnableRateLimiting("StudentSearch")]
    public async Task<IActionResult> SearchChildren(
        SearchChildrenRequest request,
        CancellationToken cancellationToken)
    {
        var captchaValid = await _turnstile.VerifyStudentSearchAsync(
            request.TurnstileToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (!captchaValid)
        {
            return BadRequest(new { message = "CAPTCHA verification failed." });
        }

        return Ok(await _students.SearchChildrenAsync(request.ParentPhone, request.ChildDob));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
    {
        var student = await _students.CreateAsync(request);
        return CreatedAtAction(nameof(GetStudents), new { id = student.Id }, student);
    }
}
