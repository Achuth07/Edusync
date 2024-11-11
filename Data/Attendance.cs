using System;
using System.ComponentModel.DataAnnotations;

namespace Edusync.Data
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student selection is required.")]
        [Display(Name = "Student Name")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Course selection is required.")]
        [Display(Name = "Course")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status should not exceed 20 characters.")]
        [RegularExpression("^(Present|Absent|Late|Excused)$", ErrorMessage = "Status must be either 'Present', 'Absent', 'Late', or 'Excused'.")]
        public string Status { get; set; } = "Absent"; // Defaults to Absent

        public virtual Student? Student { get; set; }
        public virtual Class? Class { get; set; }
    }
}
