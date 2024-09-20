using Microsoft.EntityFrameworkCore;
using workDashboard.Models;

namespace workDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Departmant> Departmants { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Work> Works { get; set; }  // Added Work DbSet
        public DbSet<Priority> Priorities { get; set; }  // Added Priority DbSet
        public DbSet<Staging> Stagings { get; set; }  // Added Staging DbSet
        public DbSet<Taxes> Taxes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Admin - Departmant relationship
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Departmant) // Admin has one Departmant
                .WithMany(d => d.Admins) // Departmant has many Admins
                .HasForeignKey(a => a.DepartmantId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to prevent Admin from being deleted when Departmant is deleted

            // Configure Company -> Employees relationship with Cascade Delete
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Employees) // Company has many Employees
                .WithOne(e => e.Company) // Employee belongs to one Company
                .HasForeignKey(e => e.CompanyId) // Foreign key in Employee
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete employees when a company is deleted

            // Configure Employee -> Works relationship with Cascade Delete
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Works) // Employee has many Works
                .WithOne(w => w.Employee) // Work belongs to one Employee
                .HasForeignKey(w => w.EmployeeId) // Foreign key in Work
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete works when an employee is deleted

            // Configure Work -> Company relationship
            modelBuilder.Entity<Work>()
                .HasOne(w => w.Company) // Work belongs to one Company
                .WithMany() // No navigation property on Company
                .HasForeignKey(w => w.CompanyId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to prevent accidental work deletion if company is deleted

            // Configure Work -> Departmant relationship
            modelBuilder.Entity<Work>()
                .HasOne(w => w.Departmant) // Work belongs to one Departmant
                .WithMany() // No navigation property on Departmant
                .HasForeignKey(w => w.DepartmantId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to prevent accidental deletion

            // Ensure Email is unique
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

    }
}
