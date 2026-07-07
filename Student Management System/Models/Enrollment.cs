using Student_Management_System.Models.Enum;
using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Enrollment
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public long ClassroomId { get; set; }

    public DateOnly EnrollDate { get; set; }

    public EnrollmentStatus Status { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Classroom Classroom { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
