namespace workDashboard.Dtos
{
    using System.ComponentModel.DataAnnotations;

    namespace workDashboard.Dtos
    {
        public class AdminUpdateDTO
        {
            public string Name { get; set; }

            public string Email { get; set; }

            public string? Password { get; set; } // Optional field

            public int DepartmantId { get; set; }
        }
    }

}
