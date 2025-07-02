using ERP.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations
{
    public class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
    {
        public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
        {
            builder.ToTable("project_assignments");

            // Primary Key
            builder.HasKey(pa => pa.Id);
            builder.Property(pa => pa.Id).HasColumnName("assignment_id");

            // Required Properties
            builder.Property(pa => pa.ProjectId)
                .IsRequired()
                .HasColumnName("project_id");

            builder.Property(pa => pa.EmployeeId)
                .IsRequired()
                .HasColumnName("employee_id");

            builder.Property(pa => pa.Role)
                .IsRequired()  // ✅ Required로 변경 (실제 DB 스키마에 맞춤)
                .HasMaxLength(100)
                .HasColumnName("role");

            builder.Property(pa => pa.AllocationPercentage)
                .IsRequired()
                .HasDefaultValue(100)
                .HasColumnName("allocation_percentage");

            builder.Property(pa => pa.HourlyRate)
                .HasColumnType("decimal(10,2)")
                .HasColumnName("hourly_rate");

            // ✅ CreatedAt 기본값 제거 (MySQL 호환성을 위해)
            builder.Property(pa => pa.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");
            // .HasDefaultValueSql("CURRENT_TIMESTAMP") // 제거

            // ✅ Value Object - DateRange (Period 속성을 통해 매핑)
            builder.OwnsOne(pa => pa.Period, period =>
            {
                period.Property(r => r.StartDate)
                    .IsRequired()
                    .HasColumnName("start_date");

                period.Property(r => r.EndDate)
                    .HasColumnName("end_date");
            });

            // Relationships
            builder.HasOne(pa => pa.Project)
                .WithMany(p => p.Assignments)
                .HasForeignKey(pa => pa.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pa => pa.Employee)
                .WithMany(e => e.Assignments)
                .HasForeignKey(pa => pa.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(pa => pa.ProjectId).HasDatabaseName("idx_project_assignments_project");
            builder.HasIndex(pa => pa.EmployeeId).HasDatabaseName("idx_project_assignments_employee");
            builder.HasIndex(pa => new { pa.ProjectId, pa.EmployeeId }).HasDatabaseName("idx_project_assignments_project_employee");
        }
    }
}