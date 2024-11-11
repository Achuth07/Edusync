using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Edusync.Data;

public partial class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Course name is required.")]
    [StringLength(100, ErrorMessage = "Course name should not exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(10, ErrorMessage = "Course code should not exceed 10 characters.")]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "Course code must contain only uppercase letters and numbers.")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "Credits are required.")]
    [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10.")]
    public int? Credits { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
