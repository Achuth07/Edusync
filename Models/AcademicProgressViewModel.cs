namespace Edusync.Models
{
    public class AcademicProgressViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public List<CourseGradeViewModel> CourseGrades { get; set; } = new List<CourseGradeViewModel>();
    }

    public class CourseGradeViewModel
    {
        public string CourseName { get; set; }
        public string AssessmentType { get; set; }
        public decimal Score { get; set; }
        public DateTime DateRecorded { get; set; }
        public string AcademicYear { get; set; } = "2024";
    }
}
