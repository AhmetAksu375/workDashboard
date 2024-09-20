using workDashboard.Models;

namespace workDashboard.Interfaces
{
    public interface ICompanyService
    {
        Task<Company> GetCompanyByIdAsync(int id);
        Task<bool> CompanyExistsByEmailAsync(string email);
    }
}
