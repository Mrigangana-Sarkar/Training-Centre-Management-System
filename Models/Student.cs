namespace TrainingCentreManagementSystem.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public string? StudentName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public int CourseId { get; set; }
    }
}