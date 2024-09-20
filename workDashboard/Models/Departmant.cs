namespace workDashboard.Models
{
    public class Departmant
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public ICollection<Admin> Admins { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
