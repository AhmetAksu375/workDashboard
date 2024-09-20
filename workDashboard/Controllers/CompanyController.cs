using Microsoft.AspNetCore.Mvc;
using workDashboard.Models;
using workDashboard.Dtos;
using workDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace workDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")] // Enforces that only authorized users with the "CompanyPolicy" can access this controller
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CompanyController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // Inject IConfiguration if needed for future use
        }

        // GET: api/Company
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetCompanies()
        {
            var companies = await _context.Companies
                .Include(c => c.Employees)  // Include employees in the query
                .Select(c => new CompanyDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Employees = c.Employees.Select(e => new EmployeeDTO
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Email = e.Email
                        // Map other properties if needed
                    }).ToList()
                })
                .ToListAsync();

            return Ok(companies);
        }



        // GET: api/Company/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Employees)  // Ensure Employees are included
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        // POST: api/Company
        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            if (company == null)
            {
                return BadRequest("Company data is null.");
            }

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
        }

        // PUT: api/Company/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(int id, Company company)
        {
            if (id != company.Id)
            {
                return BadRequest("ID mismatch.");
            }

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound($"Company with ID {id} not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Company/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            // Fetch the company including its related employees
            var company = await _context.Companies
                .Include(c => c.Employees) // Ensure employees are included
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound($"Company with ID {id} not found.");
            }

            // Handle related employees
            if (company.Employees.Any())
            {
                // Optionally, if you want to log or take action before deleting employees
                // var employees = company.Employees;

                // Remove associated employees
                _context.Employees.RemoveRange(company.Employees);
            }

            // Remove the company
            _context.Companies.Remove(company);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound($"Company with ID {id} not found.");
                }
                else
                {
                    // Rethrow the exception if it's not related to the entity not being found
                    throw;
                }
            }

            return NoContent();
        }
        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
