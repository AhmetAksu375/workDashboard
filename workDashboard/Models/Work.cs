using System.Text.Json.Serialization;

namespace workDashboard.Models
{
    public class Work
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        // Foreign keys
        public int? EmployeeId { get; set; }
        public int? CompanyId { get; set; }
        public int DepartmantId { get; set; }
        public int PriorityId { get; set; }
        public int StagingId { get; set; }

        public float Hours { get; set; }
        public int WorkerCount { get; set; }
        public float Price { get; set; }

        public Employee? Employee { get; set; }

        public Company Company { get; set; }

        public Departmant Departmant { get; set; }

        public DateTime Date_Start { get; set; }
        public DateTime Date_Finish { get; set; }
    }
}
