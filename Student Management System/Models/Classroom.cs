using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Classroom
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal TuitionFee { get; set; }

    public string? AgeGroup { get; set; }

    public string? Description { get; set; }

    public int Capacity { get; set; }

    public long? TeacherId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Teacher? Teacher { get; set; }
}
