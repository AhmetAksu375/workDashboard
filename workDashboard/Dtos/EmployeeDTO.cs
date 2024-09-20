namespace workDashboard.Dtos
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int DepartmantId { get; set; }

        public int CompanyId { get; set; }

        // Password gibi hassas bilgileri eklemiyoruz
    }
}
