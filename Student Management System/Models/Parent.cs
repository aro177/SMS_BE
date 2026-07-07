using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Parent
{
    public long Id { get; set; }

    public string Fullname { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
