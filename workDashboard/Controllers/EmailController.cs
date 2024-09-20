using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using workDashboard.Data; // Adjust the namespace as necessary
using workDashboard.Models; // Ensure this namespace includes your Work and Employee models
using workDashboard.Services;

namespace workDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public EmailController(EmailService emailService, ApplicationDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        [HttpPost("decline")]
        public async Task<IActionResult> DeclineWorkEmail([FromBody] EmailRequest request)
        {
            if (request.WorkId <= 0 || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Please provide a valid WorkId and Message.");
            }

            try
            {
                // Get the current user (admin)
                var userId = User.FindFirstValue("id");
                var departmant = User.FindFirstValue("departmant");

                if (userId == null)
                {
                    return Unauthorized("Invalid token.");
                }

                int userIdInt = int.Parse(userId); // Convert string to int
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == userIdInt);
                if (admin == null)
                {
                    return Unauthorized("Admin not found.");
                }

                // Check if the admin's department is "Yönetici"
                if (departmant == "Yönetici")
                {
                    // Fetch the specific work request
                    var workRequest = await _context.Works
                        .Include(w => w.Employee) // Include related employee data
                        .Include(w => w.Company) // Include related company data
                        .FirstOrDefaultAsync(w => w.Id == request.WorkId);

                    if (workRequest == null)
                    {
                        return NotFound("Work request not found.");
                    }

                    // Update the status of the work request to "DECLINED"
                    workRequest.Status = "Declined";
                    _context.Works.Update(workRequest);
                    await _context.SaveChangesAsync();

                    // Determine the recipient's email
                    string recipientEmail;
                    if (workRequest.Employee != null)
                    {
                        recipientEmail = workRequest.Employee.Email;
                    }
                    else if (workRequest.Company != null)
                    {
                        recipientEmail = workRequest.Company.Email;
                    }
                    else
                    {
                        return NotFound("Recipient email not found.");
                    }

                    // Send an email notifying the recipient
                    var subject = "Your Work Request Has Been Declined";
                    var body = $"Dear Recipient,\n\nYour work request with ID {request.WorkId} has been declined.\n\nMessage: {request.Message}";

                    await _emailService.SendEmailAsync(recipientEmail, subject, body);
                    return Ok("Email sent successfully.");
                }
                else
                {
                    return Unauthorized("You are not authorized to perform this action.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to decline work request: {ex.Message}");
            }
        }

        public class EmailRequest
        {
            public int WorkId { get; set; }
            public string Message { get; set; }
        }
    }
}
