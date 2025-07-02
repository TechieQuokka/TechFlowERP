using ERP.Domain.Entities;
using ERP.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ERP.Infrastructure.Persistence.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("projects");

            // Primary Key
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("project_id");

            // Required Properties
            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("project_code");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name");

            builder.Property(p => p.TenantId)
                .IsRequired()
                .HasMaxLength(36)
                .HasColumnName("tenant_id");

            builder.Property(p => p.ClientId)
                .IsRequired()
                .HasColumnName("client_id");

            builder.Property(p => p.ManagerId)
                .IsRequired()
                .HasColumnName("manager_id");

            // Enum 매핑
            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasColumnName("status");

            builder.Property(p => p.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasColumnName("project_type");

            builder.Property(p => p.RiskLevel)
                .IsRequired()
                .HasConversion<string>()
                .HasColumnName("risk_level");

            // Value Objects
            builder.OwnsOne(p => p.Period, period =>
            {
                period.Property(r => r.StartDate)
                    .IsRequired()
                    .HasColumnName("start_date");

                period.Property(r => r.EndDate)
                    .HasColumnName("end_date");
            });

            // Budget Value Object
            builder.OwnsOne(p => p.Budget, budget =>
            {
                budget.Property(m => m.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(15,2)")
                    .HasColumnName("budget");

                // Currency는 완전히 무시
                budget.Ignore(m => m.Currency);
            });

            // Optional Properties
            builder.Property(p => p.Description)
                .HasColumnName("description");

            builder.Property(p => p.ProfitMargin)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("profit_margin");

            // Timestamps
            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            // Technologies - JSON 형식으로 변경
            var technologiesConverter = new ValueConverter<IReadOnlyCollection<string>, string?>(
                // To database: JSON 배열로 변환
                v => v == null || !v.Any()
                    ? null
                    : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                // From database: JSON을 List로 변환
                v => string.IsNullOrEmpty(v)
                    ? new List<string>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new List<string>()
            );

            var technologiesProperty = builder.Property(p => p.Technologies)
                .HasField("_technologies")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasConversion(technologiesConverter)
                .HasColumnName("technologies")
                .HasColumnType("json")
                .IsRequired(false);

            // Value Comparer 설정
            technologiesProperty.Metadata.SetValueComparer(
                new ValueComparer<IReadOnlyCollection<string>>(
                    (c1, c2) => (c1 == null && c2 == null) ||
                                (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<string>() : c.ToList()
                )
            );

            // Relationships
            builder.HasOne(p => p.Client)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Manager)
                .WithMany()
                .HasForeignKey(p => p.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Assignments)
                .WithOne(a => a.Project)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Milestones)
                .WithOne(m => m.Project)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.TenantId).HasDatabaseName("idx_projects_tenant");
            builder.HasIndex(p => p.Code).HasDatabaseName("idx_projects_code");
            builder.HasIndex(p => p.ClientId).HasDatabaseName("idx_projects_client");
            builder.HasIndex(p => p.ManagerId).HasDatabaseName("idx_projects_manager");
            builder.HasIndex(p => p.Status).HasDatabaseName("idx_projects_status");
            builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique().HasDatabaseName("uk_projects_tenant_code");
        }
    }
}