using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Students;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _students;

    public StudentsController(IStudentService students)
    {
        _students = students;
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] PaginationQuery pagination)
    {
        return Ok(await _students.GetPagedAsync(pagination));
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchChildren([FromQuery] string parentPhone, [FromQuery] DateOnly childDob)
    {
        return Ok(await _students.SearchChildrenAsync(parentPhone, childDob));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
    {
        var student = await _students.CreateAsync(request);
        return CreatedAtAction(nameof(GetStudents), new { id = student.Id }, student);
    }
}
