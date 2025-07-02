using ERP.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("clients");

            // Primary Key
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("client_id");

            // Required Properties
            builder.Property(c => c.CompanyName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("company_name");

            builder.Property(c => c.TenantId)
                .IsRequired()
                .HasMaxLength(36)
                .HasColumnName("tenant_id");

            // Optional Properties
            builder.Property(c => c.Industry)
                .HasMaxLength(100)
                .HasColumnName("industry");

            builder.Property(c => c.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");

            builder.Property(c => c.ContactEmail)
                .HasMaxLength(100)
                .HasColumnName("contact_email");

            builder.Property(c => c.ContactPhone)
                .HasMaxLength(50)
                .HasColumnName("contact_phone");

            builder.Property(c => c.Address)
                .HasMaxLength(500)  // DB는 TEXT이지만 Entity에서는 500으로 제한
                .HasColumnName("address");

            builder.Property(c => c.ContractValue)
                .HasColumnType("decimal(15,2)")
                .HasColumnName("contract_value");

            // ClientSize 매핑 수정 - DB는 size_category
            builder.Property(c => c.ClientSize)
                .HasMaxLength(50)
                .HasColumnName("size_category");  // client_size → size_category

            // Timestamps
            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(c => c.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            // Relationships
            builder.HasMany(c => c.Projects)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Computed Properties 무시
            builder.Ignore(c => c.TotalProjectsCount);
            builder.Ignore(c => c.ActiveProjectsCount);
            builder.Ignore(c => c.SuccessRate);
            builder.Ignore(c => c.AverageProjectBudget);
            builder.Ignore(c => c.IsVipClient);
            builder.Ignore(c => c.IsNewClient);

            // Indexes
            builder.HasIndex(c => c.TenantId).HasDatabaseName("idx_clients_tenant");
            builder.HasIndex(c => c.CompanyName).HasDatabaseName("idx_clients_company_name");
            builder.HasIndex(c => c.Industry).HasDatabaseName("idx_clients_industry");
            builder.HasIndex(c => new { c.TenantId, c.CompanyName }).HasDatabaseName("idx_clients_tenant_company");
        }
    }
}