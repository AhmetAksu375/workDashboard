using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using workDashboard.Data;
using workDashboard.Dtos;
using workDashboard.Models;
using workDashboard.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace workDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly InvoiceService _invoiceService;
        private readonly InvoicePdfService _invoicePdfService;


        public WorkController(
        ApplicationDbContext context,
        EmailService emailService,
        InvoiceService invoiceService,
        InvoicePdfService invoicePdfService)
        {
            _context = context;
            _emailService = emailService;
            _invoiceService = invoiceService;
            _invoicePdfService = invoicePdfService;
        }

        [HttpPost]
        [Authorize(Policy = "CompanyOrEmployeePolicy")] // Ensure only users with the CompanyOrEmployeePolicy can access this endpoint
        public async Task<IActionResult> CreateWork([FromBody] WorkCreateDTO model)
        {
            // Retrieve claims from the token
            var idClaim = User.FindFirstValue("id"); // This will be either employeeId or null
            var companyIdClaim = User.FindFirstValue("companyId"); // This will be either companyId or null
            var departmentClaim = User.FindFirstValue("departmant"); // This will be department name or null
            var audience = User.FindFirstValue("aud"); // Audience can be "company" or "employee"

            if (audience == "company")
            {
                // For companies, idClaim should be present and valid
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int companyId))
                {
                    return Unauthorized("Invalid or missing id claim for company.");
                }

                var company = await _context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    return NotFound("Company not found.");
                }

                // Create Work for company
                var work = new Work
                {
                    Title = model.Title,
                    Description = model.Description,
                    EmployeeId = null, // Company does not provide EmployeeId
                    CompanyId = companyId,
                    Company = company,
                    DepartmantId = model.DepartmantId,
                    PriorityId = model.PriorityId,
                    Status = "Pending",
                };

                // Add the work to the context
                _context.Works.Add(work);
                await _context.SaveChangesAsync();

                return Ok(work);
            }
            else if (audience == "employee")
            {
                // For employees, idClaim and companyIdClaim should be present and valid
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int employeeId) ||
                    string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    return Unauthorized("Invalid or missing id or companyId claim for employee.");
                }
                var company = await _context.Companies.FindAsync(companyId);

                // Create Work for employee
                var work = new Work
                {
                    Title = model.Title,
                    Description = model.Description,
                    EmployeeId = employeeId,
                    Company = company,
                    CompanyId = companyId,
                    DepartmantId = model.DepartmantId,
                    PriorityId = model.PriorityId,
                    Status = "Pending",
                };

                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                if (work.DepartmantId != employee.DepartmantId)
                {
                    return Unauthorized("You don't have permission to add work in that department.");
                }

                // Add the work to the context
                _context.Works.Add(work);
                await _context.SaveChangesAsync();

                return Ok(work);
            }
            else
            {
                return Unauthorized("Invalid token audience.");
            }
        }



        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> UpdateWork(int id, [FromBody] WorkUpdateDTO model, [FromServices] EmailService emailService, [FromServices] InvoiceService invoiceService, [FromServices] InvoicePdfService invoicePdfService)
        {
            // Find the work item by id
            var work = await _context.Works.FindAsync(id);
            if (work == null)
            {
                return NotFound("Work item not found.");
            }

            // Check if the logged-in user is an admin and authorized to update this work item
            var userId = User.FindFirstValue("id");
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            int userIdInt = int.Parse(userId); // Converts string to int
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == userIdInt);

            // Only allow updating if admin is from the same department or if admin is in department 3 (Yönetici)
            if (work.DepartmantId != admin.DepartmantId && admin?.DepartmantId != 3)
            {
                return Unauthorized("You are not authorized to update work for this department.");
            }

            // First check if the status is "Completed" before making any changes
            if (model.Status == "Completed")
            {
                // Check if the work is already completed
                if (work.Status == "Completed")
                {
                    return BadRequest("Work item is already completed and cannot be completed again.");
                }

                // Now, proceed to update the status and the rest of the fields
                work.Status = "Completed"; // Only now update the status to "Completed"

                // Update the work item with provided values or keep the existing ones if null
                work.StagingId = model.StagingId ?? work.StagingId;
                work.Hours = model.Hours ?? work.Hours;
                work.WorkerCount = model.WorkerCount ?? work.WorkerCount;
                work.Price = model.Price ?? work.Price;
                work.Date_Start = model.Date_Start ?? work.Date_Start;
                work.Date_Finish = model.Date_Finish ?? work.Date_Finish;

                _context.Works.Update(work);
                await _context.SaveChangesAsync();

                // Handle baseAmount as a decimal
                decimal baseAmount = (decimal)work.Price;

                // Generate the invoice
                var invoice = await invoiceService.CreateInvoice(
                    work.Id,
                    work.CompanyId ?? 0,
                    work.EmployeeId ?? 0,
                    admin.Id,
                    baseAmount
                );

                // Generate PDF for the invoice
                var pdfContent = invoicePdfService.GenerateInvoicePdf(invoice);

                // Fetch employee and company details
                var employee = await _context.Employees.FindAsync(work.EmployeeId);
                var company = await _context.Companies.FindAsync(work.CompanyId);

                // Send email to employee if they exist and have a valid email
                if (employee != null && IsValidEmail(employee.Email))
                {
                    string employeeSubject = "Work Completed and Invoice Generated";
                    string employeeBody = $"Hello {employee.Name},\n\nYour work titled '{work.Title}' has been completed. Please find the attached invoice for the work completed.";

                    // Send email to employee with attached invoice PDF
                    await emailService.SendEmailWithAttachmentAsync(
                        employee.Email,
                        employeeSubject,
                        employeeBody,
                        pdfContent,
                        "Invoice.pdf"
                    );
                }

                // Send email to company if they exist and have a valid email
                if (company != null && IsValidEmail(company.Email))
                {
                    string companySubject = "Work Completed and Invoice Generated";
                    string companyBody = $"Hello,\n\nThe work titled '{work.Title}' has been completed. Please find the attached invoice for the work completed.";

                    // Send email to company with attached invoice PDF
                    await emailService.SendEmailWithAttachmentAsync(
                        company.Email,
                        companySubject,
                        companyBody,
                        pdfContent,
                        "Invoice.pdf"
                    );
                }

                return Ok(work);
            }



            else
            {
                work.Status = model.Status ?? work.Status;

            }
            // If status is not "Completed", update other fields and save the changes
            work.StagingId = model.StagingId ?? work.StagingId;
            work.Hours = model.Hours ?? work.Hours;
            work.WorkerCount = model.WorkerCount ?? work.WorkerCount;
            work.Price = model.Price ?? work.Price;
            work.Date_Start = model.Date_Start ?? work.Date_Start;
            work.Date_Finish = model.Date_Finish ?? work.Date_Finish;

            _context.Works.Update(work);
            await _context.SaveChangesAsync();

            // Find the employee who created the work
            var employeeToUpdate = await _context.Employees.FirstOrDefaultAsync(e => e.Id == work.EmployeeId);

            if (employeeToUpdate != null)
            {
                // Send an email to the employee if they exist
                string subject = "Your work has been updated";
                string body = $"Hello {employeeToUpdate.Name},\n\nYour work titled '{work.Title}' has been updated. Please check the system for more details.";

                await emailService.SendEmailAsync(employeeToUpdate.Email, subject, body);
            }
            else
            {
                // If employee is null, find the company and send the email to the company
                var companyToUpdate = await _context.Companies.FirstOrDefaultAsync(c => c.Id == work.CompanyId);
                if (companyToUpdate != null)
                {
                    string subject = "Your work has been updated";
                    string body = $"Hello {companyToUpdate.Name},\n\nYour work titled '{work.Title}' has been updated. Please check the system for more details.";

                    await emailService.SendEmailAsync(companyToUpdate.Email, subject, body);
                }
                else
                {
                    return NotFound("No employee or company associated with this work item.");
                }
            }

            return Ok(work);
        }




        [HttpGet]
        [Authorize] // Ensure only authenticated users can access this
        public async Task<IActionResult> GetAllWorks()
        {
            var audienceClaim = User.FindFirstValue("aud"); // Retrieve the audience ("admin", "company", or "employee")
            var idClaim = User.FindFirstValue("id"); // Retrieve the "id" claim (companyId for companies, employeeId for employees)
            var companyIdClaim = User.FindFirstValue("companyId"); // Retrieve the "companyId" claim for employees

            IQueryable<Work> query = _context.Works
                .Include(w => w.Company)  // Include the Company entity
                .Include(w => w.Departmant); // Include the Departmant entity

            // If the user is an admin, return all works
            if (audienceClaim == "admin")
            {
                var allWorks = await query.ToListAsync();
                return Ok(allWorks);
            }
            // If the user is a company
            else if (audienceClaim == "company" && !string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out int companyId))
            {
                // Retrieve all works for the company
                var worksForCompany = await query
                    .Where(w => w.CompanyId == companyId)
                    .ToListAsync();

                return Ok(worksForCompany);
            }
            // If the user is an employee
            else if (audienceClaim == "employee" && !string.IsNullOrEmpty(idClaim) &&
                     int.TryParse(idClaim, out int employeeId) &&
                     !string.IsNullOrEmpty(companyIdClaim) && int.TryParse(companyIdClaim, out int employeeCompanyId))
            {
                // Retrieve works only for the employee within their company
                var worksForEmployee = await query
                    .Where(w => w.EmployeeId == employeeId && w.CompanyId == employeeCompanyId)
                    .ToListAsync();

                return Ok(worksForEmployee);
            }
            else
            {
                // If the token is not valid or claims are missing
                return Unauthorized("Invalid token.");
            }
        }


        [HttpPut("complete/{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> CompleteWork(int id, [FromServices] InvoiceService invoiceService, [FromServices] EmailService emailService, [FromServices] InvoicePdfService invoicePdfService)
        {
            try
            {
                // Fetch work details
                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound("Work item not found.");
                }

                // Check if the work is already completed
                if (work.Status == "COMPLETED")
                {
                    return BadRequest("Work item is already completed and cannot be completed again.");
                }

                // Get the current user (admin)
                var userId = User.FindFirstValue("id");
                if (userId == null)
                {
                    return Unauthorized("Invalid token.");
                }

                int userIdInt = int.Parse(userId); // Converts string to int
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == userIdInt);

                if(admin.DepartmantId == 3)
                {

                }
                // Authorization check
                if (work.DepartmantId != admin?.DepartmantId && admin?.DepartmantId!=3)
                {
                    return Unauthorized("You are not authorized to complete work for this department.");
                }

                // Update work status
                work.Status = "COMPLETED";
                _context.Works.Update(work);
                await _context.SaveChangesAsync();

                // Handle baseAmount as a decimal
                decimal baseAmount = (decimal)work.Price;

                // Generate the invoice
                var invoice = await invoiceService.CreateInvoice(
                    work.Id,
                    work.CompanyId ?? 0,
                    work.EmployeeId ?? 0,
                    admin.Id,
                    baseAmount
                );

                // Generate PDF for the invoice
                var pdfContent = invoicePdfService.GenerateInvoicePdf(invoice);

                var employee = await _context.Employees.FindAsync(work.EmployeeId);
                var company = await _context.Companies.FindAsync(work.CompanyId);

                if (employee != null && IsValidEmail(employee.Email))
                {
                    string employeeSubject = "Work Completed and Invoice Generated";
                    string employeeBody = $"Hello {employee.Name},\n\nYour work titled '{work.Title}' has been completed. Please find the attached invoice for the work completed.";

                    // Send email to employee with attachment
                    await emailService.SendEmailWithAttachmentAsync(
                        employee.Email,
                        employeeSubject,
                        employeeBody,
                        pdfContent,
                        "Invoice.pdf"
                    );
                }

                if (company != null && IsValidEmail(company.Email))
                {
                    string companySubject = "Work Completed and Invoice Generated";
                    string companyBody = $"Hello,\n\nThe work titled '{work.Title}' has been completed. Please find the attached invoice for the work completed.";

                    // Send email to company with attachment
                    await emailService.SendEmailWithAttachmentAsync(
                        company.Email,
                        companySubject,
                        companyBody,
                        pdfContent,
                        "Invoice.pdf"
                    );
                }

                return Ok(work);
            }
            catch (Exception ex)
            {
                // Log the exception (add logging here)
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteWork(int id)
        {
            try
            {
                // Get the current user (admin)
                var userId = User.FindFirstValue("id");
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

                // Fetch the work details
                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound("Work item not found.");
                }

                // Check if the admin's department allows deleting all works
                if (admin.DepartmantId == 3)
                {
                    // Admin has permission to delete all works
                    _context.Works.Remove(work);
                }
                else
                {
                    // Admin can only delete works from their own department
                    if (work.DepartmantId != admin.DepartmantId)
                    {
                        return Unauthorized("You are not authorized to delete work for this department.");
                    }
                    // Delete the work if it's from the same department
                    _context.Works.Remove(work);
                }

                await _context.SaveChangesAsync();
                return Ok($"Work with ID {id} has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete work: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


    }
}
