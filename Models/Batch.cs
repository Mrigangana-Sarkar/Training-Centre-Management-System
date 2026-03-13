namespace TrainingCentreManagementSystem.Models
{
    public class Batch
    {
        public int BatchId { get; set; }

        public string? BatchName { get; set; }

        public int CourseId { get; set; }

        public int TrainerId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}