using Microsoft.AspNetCore.Mvc;
using workDashboard.Models;
using workDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using workDashboard.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using workDashboard.Dtos.workDashboard.Dtos;

namespace workDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")] // Ensure only users with the AdminPolicy can access this controller
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Admin> _passwordHasher;

        // Email regex pattern for validation
        private const string EmailRegexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

        public AdminController(ApplicationDbContext context, IPasswordHasher<Admin> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/Admin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminDTO>>> GetAdmins()
        {
            var admins = await _context.Admins
                .Select(e => new AdminDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    DepartmantId = e.DepartmantId
                })
                .ToListAsync();

            return Ok(admins);
        }

        // GET: api/Admin/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminDTO>> GetAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            var adminDto = new AdminDTO
            {
                Id = admin.Id,
                Name = admin.Name,
                Email = admin.Email,
                DepartmantId = admin.DepartmantId
            };

            return Ok(adminDto);
        }

        // POST: api/Admin
        [HttpPost]
        public async Task<ActionResult<Admin>> PostAdmin(AdminCreateDTO adminCreateDTO)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            // Validate the email format
            if (!IsValidEmail(adminCreateDTO.Email))
            {
                return BadRequest("Invalid email address format.");
            }

            // Hash the password before saving
            var hashedPassword = _passwordHasher.HashPassword(null, adminCreateDTO.Password);

            // Convert DTO to Admin model
            var admin = new Admin
            {
                Name = adminCreateDTO.Name,
                Email = adminCreateDTO.Email,
                Password = hashedPassword,
                DepartmantId = adminCreateDTO.DepartmantId
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdmin), new { id = admin.Id }, admin);
        }

        // PUT: api/Admin/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(int id, [FromBody] AdminUpdateDTO adminUpdateDTO)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID.");
            }

            // Retrieve the existing admin from the database
            var existingAdmin = await _context.Admins.FindAsync(id);
            if (existingAdmin == null)
            {
                return NotFound("Admin not found.");
            }

            // Validate the email format
            if (!IsValidEmail(adminUpdateDTO.Email))
            {
                return BadRequest("Invalid email address format.");
            }

            // Update fields based on the DTO
            existingAdmin.Name = adminUpdateDTO.Name;
            existingAdmin.Email = adminUpdateDTO.Email;
            existingAdmin.DepartmantId = adminUpdateDTO.DepartmantId;

            // Only update the password if a new password is provided
            if (!string.IsNullOrEmpty(adminUpdateDTO.Password))
            {
                existingAdmin.Password = _passwordHasher.HashPassword(existingAdmin, adminUpdateDTO.Password);
            }

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(id))
                {
                    return NotFound("Admin does not exist.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Admin/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }

        // Email validation method
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, EmailRegexPattern);
        }
    }
}
