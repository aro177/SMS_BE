using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Teachers;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/teachers")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teachers;

    public TeachersController(ITeacherService teachers)
    {
        _teachers = teachers;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeachers([FromQuery] PaginationQuery pagination)
    {
        return Ok(await _teachers.GetPagedAsync(pagination));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeacher(CreateTeacherRequest request)
    {
        var teacher = await _teachers.CreateAsync(request);
        return CreatedAtAction(nameof(GetTeachers), new { id = teacher.Id }, teacher);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateTeacher(long id, UpdateTeacherRequest request)
    {
        return await _teachers.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteTeacher(long id)
    {
        return await _teachers.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
