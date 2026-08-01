using System;
using System.Collections.Generic;
using Student_Management_System.Models.Enum;

namespace Student_Management_System.Models;

public partial class Lesson
{
    public long Id { get; set; }

    public long ClassroomId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsDeleted { get; set; }

    public bool TakeAttendanceStatus { get; set; }

    public RepeatStatus RepeatStatus { get; set; }

    public string? Code { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Classroom Classroom { get; set; } = null!;
}
