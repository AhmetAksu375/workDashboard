using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using workDashboard.Models;

public class InvoicePdfService
{
    private readonly IConverter _converter;
    private readonly ILogger<InvoicePdfService> _logger;

    public InvoicePdfService(IConverter converter, ILogger<InvoicePdfService> logger)
    {
        _converter = converter;
        _logger = logger;
    }

    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var htmlContent = $@"
        <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; }}
                    .invoice-container {{ width: 100%; max-width: 800px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }}
                    .invoice-header {{ text-align: center; font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
                    .invoice-top {{ display: flex; justify-content: space-between; margin-bottom: 20px; }}
                    .invoice-top div {{ width: 48%; }}
                    .invoice-details {{ width: 100%; border-collapse: collapse; }}
                    .invoice-details th, .invoice-details td {{ padding: 10px; border: 1px solid #ddd; text-align: left; }}
                    .invoice-details th {{ background-color: #f4f4f4; font-weight: bold; }}
                    .invoice-footer {{ margin-top: 20px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #ddd; padding-top: 10px; }}
                    .invoice-footer p {{ margin: 5px 0; }}
                    .page-break {{ page-break-before: always; }}
                    .date {{ font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='invoice-container'>
                    <div class='invoice-top'>
                        <div>
                            <strong>ReactorSoftware</strong><br>
                            123 Business St.<br>
                            Suite 400<br>
                            City, Country, 12345<br>
                            Phone: (123) 456-7890
                        </div>
                        <div style='text-align: right;'>
                            <strong>Date:</strong> {DateTime.Now.ToString("yyyy-MM-dd")}<br>
                            <strong>Invoice ID:</strong> {invoice.Id}
                        </div>
                    </div>
                    <div class='invoice-header'>
                        Invoice
                    </div>
                    <table class='invoice-details'>
                        <thead>
                            <tr>
                                <th>Company ID</th>
                                <th>Employee ID</th>
                                <th>Base Amount</th>
                                <th>Tax Amount</th>
                                <th>Total Amount</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>{invoice.CompanyId}</td>
                                <td>{invoice.EmployeeId}</td>
                                <td>{invoice.BaseAmount:C}</td>
                                <td>{invoice.TaxAmount:C}</td>
                                <td>{invoice.TotalAmount:C}</td>
                            </tr>
                        </tbody>
                    </table>
                    <div class='invoice-footer'>
                        <p>Thank you for your business!</p>
                        <p>If you have any questions, please contact us at support@reactorsoftware.com</p>
                        <p>ReactorSoftware | 123 Business St., Suite 400, City, Country, 12345</p>
                    </div>
                </div>
            </body>
        </html>";

        var pdf = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            },
            Objects = {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" },
                }
            }
        };

        try
        {
            return _converter.Convert(pdf);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating PDF: {ex.Message}");
            throw new ApplicationException("An error occurred while generating the PDF. Please try again later.");
        }
    }
}
