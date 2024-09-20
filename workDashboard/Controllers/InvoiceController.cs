using Microsoft.AspNetCore.Mvc;
using workDashboard.Models;
using workDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace workDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure the controller uses the global authorization policy
    public class InvoiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/invoice
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var companyId = User.FindFirstValue("id");
            var employeeId = User.FindFirstValue("employeeId");
            var departmantIdClaim = User.FindFirstValue("departmantId");
            var isAdmin = User.HasClaim(c => c.Type == "aud" && c.Value == "admin");

            if (companyId != null)
            {
                // If user is a company, return invoices for that company
                return await _context.Invoices
                    .Where(i => i.CompanyId.ToString() == companyId)
                    .ToListAsync();
            }
            else if (employeeId != null)
            {
                // If user is an employee, return invoices for that employee
                return await _context.Invoices
                    .Where(i => i.EmployeeId.ToString() == employeeId)
                    .ToListAsync();
            }
            else if (isAdmin)
            {
                // If user is an admin, return all invoices or based on departmantId
                return await _context.Invoices
                    .Where(i => i.AdminId == 3 ||
                                _context.Works.Any(w => w.Id == i.WorkId && w.DepartmantId.ToString() == departmantIdClaim))
                    .ToListAsync();
            }

            return Forbid(); // If user does not meet any criteria
        }

        // GET: api/invoice/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            var companyId = User.FindFirstValue("id");
            var employeeId = User.FindFirstValue("employeeId");
            var departmantIdClaim = User.FindFirstValue("departmantId");
            var isAdmin = User.HasClaim(c => c.Type == "aud" && c.Value == "admin");

            // Authorization logic
            if (companyId != null && invoice.CompanyId.ToString() == companyId)
            {
                return Ok(invoice);
            }
            else if (employeeId != null && invoice.EmployeeId.ToString() == employeeId)
            {
                return Ok(invoice);
            }
            else if (isAdmin)
            {
                var work = await _context.Works.FindAsync(invoice.WorkId);
                if (work != null && (work.DepartmantId.ToString() == departmantIdClaim || invoice.AdminId == 3))
                {
                    return Ok(invoice);
                }
            }

            return Forbid(); // If user does not meet any criteria
        }
    }
}
