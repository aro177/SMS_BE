using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Student
{
    public long Id { get; set; }

    public string Fullname { get; set; } = null!;

    public DateOnly? Dob { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public long? ParentId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Parent? Parent { get; set; }
}
