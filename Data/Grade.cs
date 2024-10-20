using System;

namespace Edusync.Data
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public string AssessmentType { get; set; } = null!; // Quiz, Midterm, Final, etc.
        public decimal Score { get; set; } // Grade Score
        public DateTime DateRecorded { get; set; } = DateTime.Now;
        public string AcademicYear { get; set; } = "2024"; // Default value

        public virtual Student? Student { get; set; }
        public virtual Class? Class { get; set; }
    }
}
