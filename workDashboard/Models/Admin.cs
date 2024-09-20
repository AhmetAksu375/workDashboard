namespace workDashboard.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int DepartmantId { get; set; }

        public Departmant Departmant { get; set; }

    }
}
