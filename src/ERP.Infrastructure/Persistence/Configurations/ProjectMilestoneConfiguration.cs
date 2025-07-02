using ERP.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations
{
    public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
    {
        public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
        {
            builder.ToTable("project_milestones");

            // Primary Key
            builder.HasKey(pm => pm.Id);
            builder.Property(pm => pm.Id).HasColumnName("milestone_id");

            // Required Properties
            builder.Property(pm => pm.ProjectId)
                .IsRequired()
                .HasColumnName("project_id");

            builder.Property(pm => pm.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name");

            builder.Property(pm => pm.DueDate)
                .IsRequired()
                .HasColumnName("due_date");

            builder.Property(pm => pm.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasColumnName("status");

            builder.Property(pm => pm.PaymentPercentage)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("payment_percentage");

            builder.Property(pm => pm.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            // Optional Properties
            builder.Property(pm => pm.Description)
                .HasColumnName("description");

            builder.Property(pm => pm.CompletionDate)
                .HasColumnName("completion_date");

            builder.Property(pm => pm.Deliverables)
                .HasColumnName("deliverables");

            // Relationships
            builder.HasOne(pm => pm.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pm => pm.ProjectId).HasDatabaseName("idx_project_milestones_project");
            builder.HasIndex(pm => new { pm.ProjectId, pm.DueDate }).HasDatabaseName("idx_project_milestones_project_due");
            builder.HasIndex(pm => pm.Status).HasDatabaseName("idx_project_milestones_status");
        }
    }
}