using ERP.Domain.Entities;
using ERP.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("employees");

            // Primary Key
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("employee_id");

            // Required Properties
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("email");

            builder.Property(e => e.TenantId)
                .IsRequired()
                .HasMaxLength(36)
                .HasColumnName("tenant_id");

            builder.Property(e => e.HireDate)
                .IsRequired()
                .HasColumnName("hire_date");

            // Status 매핑 수정: is_active BOOLEAN과 매핑
            builder.Property(e => e.Status)
                .IsRequired()
                .HasColumnName("is_active")  // status → is_active
                .HasConversion(
                    v => v == EmployeeStatus.Active,  // EmployeeStatus → bool
                    v => v ? EmployeeStatus.Active : EmployeeStatus.Inactive  // bool → EmployeeStatus
                );

            // Optional Properties
            builder.Property(e => e.DepartmentId)
                .HasColumnName("department_id");

            builder.Property(e => e.ManagerId)
                .HasColumnName("manager_id");

            // Position 무시 - DB에는 position_id(외래키)가 있지만 Entity는 string
            builder.Ignore(e => e.Position);

            builder.Property(e => e.Salary)
                .HasColumnType("decimal(10,2)")
                .HasColumnName("salary");

            builder.Property(e => e.LeaveBalance)
                .HasColumnName("leave_balance");

            // Timestamps
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            // Self-referencing relationship (Manager)
            builder.HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            builder.HasMany(e => e.Skills)
                .WithOne()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Assignments)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Computed Properties 무시
            builder.Ignore(e => e.PrimarySkills);
            builder.Ignore(e => e.IsOverallocated);
            builder.Ignore(e => e.IsAssignable);

            // Indexes
            builder.HasIndex(e => e.TenantId).HasDatabaseName("idx_employees_tenant");
            builder.HasIndex(e => e.Email).HasDatabaseName("idx_employees_email");
            builder.HasIndex(e => new { e.TenantId, e.Email }).IsUnique().HasDatabaseName("uk_employees_tenant_email");
        }
    }
}