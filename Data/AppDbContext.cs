using Microsoft.EntityFrameworkCore;
using LeaveManagementApi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LeaveManagementApi.Data
{
    public class AppDbContext : DbContext
    {
        // Constructeur avec injection de dépendances
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Définition des tables
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        // Seed de données initiales
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, FullName = "Sarra Touffehi", Department = "IT", JoiningDate = new DateTime(2023, 1, 15) },
                new Employee { Id = 2, FullName = "Hajer Touffehi", Department = "RH", JoiningDate = new DateTime(2022, 9, 5) }
            );

            modelBuilder.Entity<LeaveRequest>().HasData(
                new LeaveRequest
                {
                    Id = 1,
                    EmployeeId = 1,
                    LeaveType = LeaveType.Annual,
                    StartDate = new DateTime(2024, 6, 1),
                    EndDate = new DateTime(2024, 6, 10),
                    Status = LeaveStatus.Approved,
                    Reason = "Vacances",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
