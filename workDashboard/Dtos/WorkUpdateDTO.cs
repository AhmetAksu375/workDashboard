namespace workDashboard.Dtos
{
    public class WorkUpdateDTO
    {
        public string Status { get; set; }
        public int? StagingId { get; set; }
        public float? Hours { get; set; }
        public int? WorkerCount { get; set; }
        public float? Price { get; set; }

        public DateTime? Date_Start { get; set; }

        public DateTime? Date_Finish { get; set; }
    }
}
