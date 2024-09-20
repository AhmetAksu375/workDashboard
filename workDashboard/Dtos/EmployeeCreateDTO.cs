namespace workDashboard.Dtos
{
    public class EmployeeCreateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
           
        public string Password { get; set; }
        public int DepartmantId { get; set; }

    }
}
