using Microsoft.EntityFrameworkCore;
using PhoneBookMicroservices.Shared.Models;
using System.Collections.Generic;

namespace ReportService.Report.API.Infrastructure
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

        public DbSet<ContactReport> Reports { get; set; }
        public DbSet<ReportDetail> ReportDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ContactReport ile ReportDetail arasındaki ilişkiyi belirtme
            modelBuilder.Entity<ContactReport>()
                .HasMany(cr => cr.Details)  // ContactReport, birden çok ReportDetail içerir
                .WithOne(rd => rd.ContactReport)  // Her ReportDetail, bir ContactReport'a aittir
                .HasForeignKey(rd => rd.ContactReportId);  // Yabancı anahtar
        }
    }
}
