using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Lessons;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessons;

    public LessonsController(ILessonService lessons)
    {
        _lessons = lessons;
    }

    [Authorize(Roles = "TEACHER")]
    [HttpGet("my/today")]
    public async Task<IActionResult> GetMyLessonsToday([FromQuery] DateOnly? date)
    {
        var lessons = await _lessons.GetTodayForCurrentTeacherAsync(date);
        return lessons is null ? NotFound("Teacher profile is not linked to this account.") : Ok(lessons);
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] long? teacherId,
        [FromQuery] long? classroomId,
        [FromQuery] PaginationQuery pagination)
    {
        var filter = new LessonFilter(from, to, teacherId, classroomId);
        return Ok(await _lessons.GetPagedAsync(filter, pagination));
    }

    [HttpPost]
    public async Task<IActionResult> CreateLesson(CreateLessonRequest request)
    {
        var lesson = await _lessons.CreateAsync(request);
        return lesson is null ? BadRequest("Classroom not found or invalid lesson time.") : CreatedAtAction(nameof(GetLessons), new { id = lesson.Id }, lesson);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateLesson(long id, UpdateLessonRequest request)
    {
        return await _lessons.UpdateAsync(id, request) ? NoContent() : BadRequest("Lesson/classroom not found or invalid lesson time.");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteLesson(long id)
    {
        return await _lessons.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
