using ERP.Domain.Entities;
using ERP.SharedKernel.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence
{
    public class ErpDbContext : DbContext
    {
        private readonly ICurrentTenantService _currentTenantService;

        public ErpDbContext(DbContextOptions<ErpDbContext> options, ICurrentTenantService currentTenantService)
            : base(options)
        {
            _currentTenantService = currentTenantService;
        }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
        public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
        public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // 경고 무시
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning));
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantForNewEntities();
            await PublishDomainEventsAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantForNewEntities()
        {
            var tenantId = _currentTenantService.TenantId ?? "default-tenant";
            if (string.IsNullOrEmpty(tenantId)) return;

            var addedEntries = ChangeTracker.Entries<IAggregateRoot>()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            foreach (var entry in addedEntries)
            {
                if (string.IsNullOrEmpty(entry.Entity.TenantId))
                {
                    entry.Property(nameof(IAggregateRoot.TenantId)).CurrentValue = tenantId;
                }
            }
        }

        private async Task PublishDomainEventsAsync()
        {
            var domainEntities = ChangeTracker
                .Entries<IAggregateRoot>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            foreach (var entity in domainEntities)
            {
                entity.Entity.ClearDomainEvents();
            }
        }
    }
}
