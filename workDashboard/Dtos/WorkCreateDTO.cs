namespace workDashboard.Dtos
{
    public class WorkCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public int PriorityId { get; set; }

        public int DepartmantId { get; set; }
    }
}
