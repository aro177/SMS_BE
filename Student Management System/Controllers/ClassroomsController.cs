using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Classrooms;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/classrooms")]
public class ClassroomsController : ControllerBase
{
    private readonly IClassroomService _classrooms;

    public ClassroomsController(IClassroomService classrooms)
    {
        _classrooms = classrooms;
    }

    [HttpGet]
    public async Task<IActionResult> GetClassrooms([FromQuery] PaginationQuery pagination)
    {
        return Ok(await _classrooms.GetPagedAsync(pagination));
    }

    [HttpPost]
    public async Task<IActionResult> CreateClassroom(CreateClassroomRequest request)
    {
        var classroom = await _classrooms.CreateAsync(request);
        return CreatedAtAction(nameof(GetClassrooms), new { id = classroom.Id }, classroom);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateClassroom(long id, UpdateClassroomRequest request)
    {
        return await _classrooms.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteClassroom(long id)
    {
        return await _classrooms.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
