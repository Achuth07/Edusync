using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Edusync.Data;

public partial class Class
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Teacher selection is required.")]
    public int? TeachersId { get; set; }

    [Required(ErrorMessage = "Course selection is required.")]
    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Time is required.")]
    [DataType(DataType.Time)]
    public TimeOnly? Time { get; set; }

    [Required(ErrorMessage = "Day is required.")]
    [StringLength(10, ErrorMessage = "Day should not exceed 10 characters.")]
    [RegularExpression("^(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)$", 
        ErrorMessage = "Day must be a valid day of the week (e.g., Monday, Tuesday).")]
    public string Day { get; set; } = "Monday";

    public virtual Course? Course { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Teacher? Teachers { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
