using CrudLearning.Api.Models;
using CrudLearning.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<AttendanceEntry> AttendanceEntries => Set<AttendanceEntry>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(employee => employee.Id);
                entity.Property(employee => employee.FullName).IsRequired().HasMaxLength(160);
                entity.Property(employee => employee.Email).IsRequired().HasMaxLength(160);
                entity.Property(employee => employee.Department).IsRequired().HasMaxLength(120);
                entity.Property(employee => employee.Position).IsRequired().HasMaxLength(120);
                entity.Property(employee => employee.WorkStatus).HasConversion<string>().HasMaxLength(32);
                entity.Property(employee => employee.AttendanceState).HasConversion<string>().HasMaxLength(32);
                entity.HasIndex(employee => employee.Email).IsUnique();
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Username).IsRequired().HasMaxLength(100);
                entity.Property(user => user.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32);
                entity.HasIndex(user => user.Username).IsUnique();
                entity.HasIndex(user => user.EmployeeId).IsUnique();

                entity.HasOne(user => user.Employee)
                    .WithOne(employee => employee.UserAccount)
                    .HasForeignKey<AppUser>(user => user.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AttendanceEntry>(entity =>
            {
                entity.ToTable("AttendanceEntries");
                entity.HasKey(entry => entry.Id);
                entity.Property(entry => entry.EventType).HasConversion<string>().HasMaxLength(32);
                entity.Property(entry => entry.OccurredAt).IsRequired();
                entity.Property(entry => entry.Note).HasMaxLength(250);
                entity.HasOne(entry => entry.Employee)
                    .WithMany(employee => employee.AttendanceEntries)
                    .HasForeignKey(entry => entry.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(log => log.Id);
                entity.Property(log => log.Action).IsRequired().HasMaxLength(80);
                entity.Property(log => log.Description).IsRequired().HasMaxLength(500);
                entity.Property(log => log.IpAddress).HasMaxLength(64);
                entity.Property(log => log.UserAgent).HasMaxLength(300);
                entity.HasIndex(log => log.CreatedAt);
                entity.HasIndex(log => log.ActorUserId);
                entity.HasIndex(log => log.TargetEmployeeId);
            });
        }
    }
}
