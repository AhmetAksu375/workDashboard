using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using workDashboard.Data;
using workDashboard.Interfaces;
using workDashboard.Models;
using workDashboard.Dtos;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace workDashboard.Controllers
{
    [ApiController]
    [Route("api/register/company")]
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompanyService _companyService;
        private readonly IPasswordHasher<Company> _passwordHasher;

        // Regex pattern for email validation
        private const string EmailRegexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

        public RegisterController(ApplicationDbContext context, ICompanyService companyService, IPasswordHasher<Company> passwordHasher)
        {
            _context = context;
            _companyService = companyService;
            _passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        [HttpPost("")]
        public async Task<ActionResult<CompanyDTO>> RegisterCompany([FromBody] CompanyRegisterDTO companyDto)
        {
            if (companyDto == null)
            {
                return BadRequest("Company data is null.");
            }

            // Validate the email format
            if (!IsValidEmail(companyDto.Email))
            {
                return BadRequest("Invalid email format.");
            }

            // Check if company with the same email already exists
            if (await _companyService.CompanyExistsByEmailAsync(companyDto.Email))
            {
                return BadRequest("A company with this email already exists.");
            }

            // Check if email is already taken by an admin or employee
            var employeeExists = _context.Employees.Any(e => e.Email == companyDto.Email);
            var adminExists = _context.Admins.Any(a => a.Email == companyDto.Email);

            if (adminExists || employeeExists)
            {
                return BadRequest("This email is already taken.");
            }

            // Get the department (assuming "Yönetici" exists)
            var department = _context.Departmants.FirstOrDefault(d => d.Name == "Yönetici");
            if (department == null)
            {
                return BadRequest("Department 'Yönetici' not found.");
            }

            // Create and populate the new Company object
            var company = new Company
            {
                Name = companyDto.Name,
                Email = companyDto.Email,
                DepartmantId = department.Id
            };

            // Hash the password
            company.Password = _passwordHasher.HashPassword(company, companyDto.Password);

            // Save the new company to the database
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Map to CompanyDTO
            var result = new CompanyDTO
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email
            };

            // Generate the location URL and return the response
            var location = Url.Action("GetCompany", "Company", new { id = company.Id }, Request.Scheme);
            return Created(location, result);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // Email validation using the regex pattern
                return Regex.IsMatch(email, EmailRegexPattern);
            }
            catch
            {
                return false;
            }
        }
    }
}
