using ERP.SharedKernel.Abstractions;

namespace ERP.Domain.Events
{
    public class ProjectCreatedEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType { get; } = nameof(ProjectCreatedEvent);

        public Guid ProjectId { get; }
        public string ProjectName { get; }
        public Guid ClientId { get; }
        public string TenantId { get; }

        public ProjectCreatedEvent(Guid projectId, string projectName, Guid clientId, string tenantId)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            ClientId = clientId;
            TenantId = tenantId;
        }
    }
}
