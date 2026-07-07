using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Lesson
{
    public long Id { get; set; }

    public long ClassroomId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Classroom Classroom { get; set; } = null!;
}
