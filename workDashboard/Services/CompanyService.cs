using workDashboard.Models;
using workDashboard.Data;
using workDashboard.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace workDashboard.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;

        public CompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompanyByIdAsync(int id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task<bool> CompanyExistsByEmailAsync(string email)
        {
            return await _context.Companies.AnyAsync(c => c.Email == email);
        }
    }
}
