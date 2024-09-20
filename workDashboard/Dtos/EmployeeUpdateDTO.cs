namespace workDashboard.Dtos
{
    public class EmployeeUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? DepartmantId { get; set; } // Nullable to indicate no change
        public string? Password { get; set; } // Nullable to indicate no change
    }
}
