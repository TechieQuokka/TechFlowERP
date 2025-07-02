using ERP.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ERP.Infrastructure.Persistence.Configurations
{
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("time_entries");
            // Primary Key
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("entry_id");
            // Required Properties
            builder.Property(t => t.TenantId)
                .IsRequired()
                .HasMaxLength(36)
                .HasColumnName("tenant_id");
            builder.Property(t => t.EmployeeId)
                .IsRequired()
                .HasColumnName("employee_id");
            builder.Property(t => t.ProjectId)
                .IsRequired()
                .HasColumnName("project_id");
            builder.Property(t => t.Date)
                .IsRequired()
                .HasColumnName("date");
            builder.Property(t => t.Hours)
                .IsRequired()
                .HasColumnType("decimal(4,2)")
                .HasColumnName("hours");
            builder.Property(t => t.Billable)
                .IsRequired()
                .HasColumnName("billable");
            builder.Property(t => t.Approved)
                .IsRequired()
                .HasColumnName("approved");
            // Optional Properties
            builder.Property(t => t.TaskDescription)
                .HasColumnName("task_description");
            builder.Property(t => t.ApprovedBy)
                .HasColumnName("approved_by");
            builder.Property(t => t.ApprovedAt)
                .HasColumnName("approved_at");
            // Timestamps
            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            // UpdatedAt 속성 무시 (데이터베이스에 updated_at 컬럼이 없음)
            builder.Ignore(t => t.UpdatedAt);

            // Relationships
            builder.HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
            // Indexes
            builder.HasIndex(t => t.TenantId).HasDatabaseName("idx_time_entries_tenant");
            builder.HasIndex(t => new { t.EmployeeId, t.Date }).HasDatabaseName("idx_time_entries_employee_date");
            builder.HasIndex(t => new { t.ProjectId, t.Date }).HasDatabaseName("idx_time_entries_project_date");
            builder.HasIndex(t => new { t.Billable, t.Approved }).HasDatabaseName("idx_time_entries_billable_approved");
            builder.HasIndex(t => t.ApprovedBy).HasDatabaseName("idx_time_entries_approved_by");
        }
    }
}