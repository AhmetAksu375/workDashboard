using workDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace workDashboard.Services
{
    public class TaxService
    {
        private readonly ApplicationDbContext _context;

        public TaxService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vergi türüne göre vergi oranını getir
        public async Task<decimal> GetTaxRateAsync(string taxType)
        {
            var tax = await _context.Taxes.FirstOrDefaultAsync(t => t.Type == taxType);
            return tax != null ? tax.Rate : 0m;
        }

        // Vergileri güncelleme
        public async Task UpdateTaxRateAsync(string taxType, decimal newRate)
        {
            var tax = await _context.Taxes.FirstOrDefaultAsync(t => t.Type == taxType);
            if (tax != null)
            {
                tax.Rate = newRate;
                tax.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
