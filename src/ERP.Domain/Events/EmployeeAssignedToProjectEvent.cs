using ERP.SharedKernel.Abstractions;

namespace ERP.Domain.Events
{
    public class EmployeeAssignedToProjectEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType { get; } = nameof(EmployeeAssignedToProjectEvent);

        public Guid ProjectId { get; }
        public Guid EmployeeId { get; }
        public string Role { get; }
        public int AllocationPercentage { get; }
        public string TenantId { get; }

        public EmployeeAssignedToProjectEvent(Guid projectId, Guid employeeId, string role,
            int allocationPercentage, string tenantId)
        {
            ProjectId = projectId;
            EmployeeId = employeeId;
            Role = role;
            AllocationPercentage = allocationPercentage;
            TenantId = tenantId;
        }
    }
}
