using Microsoft.AspNetCore.Mvc;
using workDashboard.Models;
using workDashboard.Dtos;
using workDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace workDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure the controller uses the global authorization policy
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private const string EmailRegexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

        public EmployeeController(ApplicationDbContext context, IPasswordHasher<Employee> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetEmployees()
        {
            var isAdmin = User.HasClaim(c => c.Type == "aud" && c.Value == "admin");
            var companyId = User.FindFirstValue("id");

            if (isAdmin)
            {
                var employees = await _context.Employees
                    .Select(e => new EmployeeDTO
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Email = e.Email,
                        DepartmantId = e.DepartmantId,
                        CompanyId = e.CompanyId
                    })
                    .ToListAsync();

                return Ok(employees);
            }

            if (companyId == null)
            {
                return Unauthorized();
            }

            var employeesByCompany = await _context.Employees
                .Where(e => e.CompanyId.ToString() == companyId)
                .Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    DepartmantId = e.DepartmantId,
                    CompanyId = e.CompanyId
                })
                .ToListAsync();

            return Ok(employeesByCompany);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var isAdmin = User.HasClaim(c => c.Type == "aud" && c.Value == "admin");
            var companyIdClaim = User.FindFirstValue("id");

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                return Ok(employee);
            }

            if (companyIdClaim == null || employee.CompanyId.ToString() != companyIdClaim)
            {
                return Forbid();
            }

            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(EmployeeCreateDTO employeeDTO)
        {
            // Check if the user has the required claim indicating they are a Company
            if (User.HasClaim(c => c.Type == "aud" && c.Value == "company"))
            {
                // Retrieve the CompanyId from the token
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
                {
                    return Forbid(); // Return Forbidden if CompanyId is not present or invalid
                }

                // Validate the email format
                if (!IsValidEmail(employeeDTO.Email))
                {
                    return BadRequest("Invalid email format.");
                }

                // Validate the password
                if (string.IsNullOrWhiteSpace(employeeDTO.Password))
                {
                    return BadRequest("Password is required.");
                }

                // Map DTO to model
                var employee = new Employee
                {
                    Name = employeeDTO.Name,
                    Email = employeeDTO.Email,
                    DepartmantId = employeeDTO.DepartmantId,
                    CompanyId = companyId, // Use the CompanyId from the token
                    Password = _passwordHasher.HashPassword(null, employeeDTO.Password) // Hash the password
                };

                // Add the employee to the context
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Return the newly created employee
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }

            // Return Forbidden if the user does not have the right claim
            return Forbid();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, EmployeeUpdateDTO employee)
        {
            if (id != employee.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var isAdmin = User.HasClaim(c => c.Type == "aud" && c.Value == "admin");
            var companyIdClaim = User.FindFirstValue("id");

            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                return NotFound();
            }

            // Check if the user is authorized
            if (isAdmin || (companyIdClaim != null && existingEmployee.CompanyId.ToString() == companyIdClaim))
            {
                // Update fields
                existingEmployee.Name = employee.Name ?? existingEmployee.Name;
                existingEmployee.Email = employee.Email ?? existingEmployee.Email;

                // Update DepartmantId only if a new value is provided
                if (employee.DepartmantId.HasValue)
                {
                    existingEmployee.DepartmantId = employee.DepartmantId.Value;
                }

                // Update password only if it's provided and not empty
                if (!string.IsNullOrWhiteSpace(employee.Password))
                {
                    existingEmployee.Password = _passwordHasher.HashPassword(existingEmployee, employee.Password);
                }

                _context.Entry(existingEmployee).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }

            return Forbid();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var isAdmin = User.HasClaim(c => c.Type == "aud" && c.Value == "admin");
            var companyIdClaim = User.FindFirstValue("id");

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return NoContent();
            }

            if (companyIdClaim == null || employee.CompanyId.ToString() != companyIdClaim)
            {
                return Forbid();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, EmailRegexPattern);
        }
    }
}
