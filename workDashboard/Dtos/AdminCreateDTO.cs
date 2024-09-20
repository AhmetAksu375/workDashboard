using System.ComponentModel.DataAnnotations;

namespace workDashboard.Dtos
{
    public class AdminCreateDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int DepartmantId { get; set; }
    }
}