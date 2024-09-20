namespace workDashboard.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int DepartmantId { get; set; }

        public string Password { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();  // Ensure it is initialized


    }
}
