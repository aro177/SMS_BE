using Student_Management_System.Models.Enum;
using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Attendance
{
    public long Id { get; set; }

    public long LessonId { get; set; }

    public long StudentId { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Note { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
