using workDashboard.Data;
using workDashboard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace workDashboard.Services
{
    public class InvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly TaxService _taxService;

        public InvoiceService(ApplicationDbContext context, EmailService emailService, TaxService taxService)
        {
            _context = context;
            _emailService = emailService;
            _taxService = taxService;
        }

        public async Task<Invoice> CreateInvoice(int workId, int companyId, int? employeeId, int adminId, decimal baseAmount)
        {
            // Fetch tax rates from the database
            decimal vat = await _taxService.GetTaxRateAsync("KDV"); // KDV
            decimal withholdingTax = await _taxService.GetTaxRateAsync("Stopaj"); // Stopaj
            decimal stampDuty = await _taxService.GetTaxRateAsync("DamgaVergisi"); // Damga Vergisi

            // Tax calculation
            decimal vatAmount = baseAmount * vat / 100;
            decimal withholdingTaxAmount = baseAmount * withholdingTax / 100;
            decimal stampDutyAmount = baseAmount * stampDuty / 100;
            decimal taxAmount = vatAmount + withholdingTaxAmount + stampDutyAmount;
            decimal totalAmount = baseAmount + taxAmount;

            // Create invoice
            var invoice = new Invoice
            {
                WorkId = workId,
                CompanyId = companyId,
                EmployeeId = employeeId.GetValueOrDefault(), // Convert to int
                AdminId = adminId,
                BaseAmount = baseAmount,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                CreatedDate = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Fetch employee and company details
            Employee employee = null;
            if (employeeId.HasValue)
            {
                employee = await _context.Employees.FindAsync(employeeId.Value);
            }

            var company = await _context.Companies.FindAsync(companyId);

            // Prepare the email content in English
            string emailBody = $"Invoice Details:\n" +
                               $"Total Amount: {totalAmount:C}\n" +
                               $"Taxes: {taxAmount:C}\n" +
                               $"Base Amount (Excluding Taxes): {baseAmount:C}\n" +
                               $"VAT ({vat}%): {vatAmount:C}\n" +
                               $"Withholding Tax ({withholdingTax}%): {withholdingTaxAmount:C}\n" +
                               $"Stamp Duty ({stampDuty}%): {stampDutyAmount:C}\n" +
                               $"Issued on: {DateTime.Now:MMMM dd, yyyy}";

            // Send the invoice details via email to both the company and employee
            if (employee != null)
            {
                await _emailService.SendEmailAsync(employee.Email, "Invoice Details", emailBody);
            }

            //if (company != null)
            //{
            //    await _emailService.SendEmailAsync(company.Email, "Invoice Details", emailBody);
            //}

            return invoice;
        }

    }
}
