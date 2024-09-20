namespace workDashboard.Dtos
{
    public class CompanyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public List<EmployeeDTO> Employees { get; set; } = new List<EmployeeDTO>();  // Initialize to avoid null

    }
}
