using System;
using System.ComponentModel.DataAnnotations;

namespace Edusync.Data
{
    public class Grade
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Class ID is required.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Assessment Type is required.")]
        [StringLength(50, ErrorMessage = "Assessment Type cannot exceed 50 characters.")]
        [RegularExpression("^(Quiz|Midterm|Final|Homework|Project)$", ErrorMessage = "Invalid Assessment Type. Allowed values are Quiz, Midterm, Final, Homework, or Project.")]
        public string AssessmentType { get; set; } = null!; // Quiz, Midterm, Final, etc.

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public decimal Score { get; set; } // Grade Score

        [DataType(DataType.Date)]
        public DateTime DateRecorded { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Academic Year is required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Academic Year must be a 4-digit year.")]
        public string AcademicYear { get; set; } = "2024"; // Default value

        public virtual Student? Student { get; set; }
        public virtual Class? Class { get; set; }
    }
}
