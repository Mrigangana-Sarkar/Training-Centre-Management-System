namespace TrainingCentreManagementSystem.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }

        public int BatchId { get; set; }

        public DateTime TrainingDate { get; set; }

        public string? Topic { get; set; }
    }
}
