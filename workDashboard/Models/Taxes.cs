namespace workDashboard.Models
{
    public class Taxes
    {
        public int Id { get; set; }
        public string Type { get; set; } // Örneğin, "KDV", "Stopaj", "DamgaVergisi"
        public decimal Rate { get; set; }   // Vergi oranı
        public DateTime LastUpdated { get; set; } // Son güncellenme tarihi
    }
}
