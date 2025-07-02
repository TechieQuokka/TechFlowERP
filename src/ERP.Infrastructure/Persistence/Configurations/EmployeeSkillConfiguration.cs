using ERP.Domain.Entities;
using ERP.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations
{
    public class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
    {
        public void Configure(EntityTypeBuilder<EmployeeSkill> builder)
        {
            builder.ToTable("employee_skills");

            // Primary Key
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasColumnName("skill_id");

            // Foreign Key
            builder.Property(s => s.EmployeeId).HasColumnName("employee_id");

            // Properties
            builder.Property(s => s.Technology)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("technology");

            builder.Property(s => s.YearsExperience)
                .HasColumnName("years_experience");

            builder.Property(s => s.LastUsedDate)
                .HasColumnName("last_used_date");

            builder.Property(s => s.Certification)
                .HasMaxLength(200)
                .HasColumnName("certification");

            // Enum 매핑
            builder.Property(s => s.Level)
                .HasColumnName("skill_level")
                .HasConversion<string>();

            // 먼저 CreatedAt을 무시 (중복 매핑 방지)
            builder.Ignore(s => s.CreatedAt);

            // Relationships
            builder.HasOne<Employee>()
                .WithMany(e => e.Skills)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(s => s.EmployeeId).HasDatabaseName("idx_employee_skills_employee");
            builder.HasIndex(s => s.Technology).HasDatabaseName("idx_employee_skills_technology");
            builder.HasIndex(s => new { s.EmployeeId, s.Technology })
                .IsUnique()
                .HasDatabaseName("uk_employee_skills_employee_tech");
        }
    }
}