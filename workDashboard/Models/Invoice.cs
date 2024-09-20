namespace workDashboard.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int WorkId { get; set; }
        public int CompanyId { get; set; }
        public int? EmployeeId { get; set; }
        public int AdminId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public bool PaymentPaid { get; set; }  // New property to track if payment has been made
        public DateTime CreatedDate { get; set; }
    }
}
