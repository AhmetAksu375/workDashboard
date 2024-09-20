namespace workDashboard.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }

        public int DepartmantId { get; set; }

        public int CompanyId { get; set; }

        public Departmant Departmant { get; set; }
        public Company Company { get; set; }
        public ICollection<Work> Works { get; set; } = new List<Work>();  // Work ilişkisini




    }
}
