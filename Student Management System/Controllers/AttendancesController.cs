using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Dtos.Attendances;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Controllers;

[ApiController]
[Route("api/attendances")]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _attendances;

    public AttendancesController(IAttendanceService attendances)
    {
        _attendances = attendances;
    }

    [HttpGet("lesson/{lessonId:long}")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> GetLessonRoster(long lessonId)
    {
        var roster = await _attendances.GetLessonRosterAsync(lessonId);
        return roster is null ? NotFound() : Ok(roster);
    }

    [HttpPut("lesson/{lessonId:long}")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> MarkLesson(long lessonId, BulkAttendanceRequest request)
    {
        return await _attendances.MarkLessonAsync(lessonId, request) switch
        {
            MarkAttendanceResult.Success => NoContent(),
            MarkAttendanceResult.AttendanceLocked => Conflict(new
            {
                message = "Attendance is locked for this lesson."
            }),
            _ => NotFound()
        };
    }

    [HttpGet("student/{studentId:long}")]
    public async Task<IActionResult> GetStudentHistory(long studentId)
    {
        return Ok(await _attendances.GetStudentHistoryAsync(studentId));
    }
}
