using System;
using System.ComponentModel.DataAnnotations;

namespace Edusync.Data
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        [Display(Name ="Student Name")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name ="Course")]
        public int ClassId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Absent"; // Defaults to Absent

        //public bool IsPresent { get; set; }

        public virtual Student? Student { get; set; }
        public virtual Class? Class { get; set; }
    }
}
