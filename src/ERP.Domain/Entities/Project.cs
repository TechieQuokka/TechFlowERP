using System.ComponentModel.DataAnnotations;

using ERP.Domain.Enums;
using ERP.Domain.Events;
using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class Project : BaseEntity
    {
        private readonly List<ProjectAssignment> _assignments = new();
        private readonly List<ProjectMilestone> _milestones = new();
        private readonly List<string> _technologies = new();
        private Money _budget = Money.Zero("USD");

        private Project() { } // EF Core

        public Project(string tenantId, ProjectCode code, string name, Guid clientId,
            Guid managerId, DateRange period, Money? budget = null) : base(tenantId)
        {
            Guard.AgainstNull(code, nameof(code));
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstInvalidGuid(clientId, nameof(clientId));
            Guard.AgainstInvalidGuid(managerId, nameof(managerId));
            Guard.AgainstNull(period, nameof(period));

            Code = code;
            Name = name;
            ClientId = clientId;
            ManagerId = managerId;
            Period = period;
            Budget = budget ?? Money.Zero();
            Status = ProjectStatus.Planning;
            Type = ProjectType.TimeAndMaterial;
            RiskLevel = RiskLevel.Medium;

            AddDomainEvent(new ProjectCreatedEvent(Id, name, clientId, tenantId));
        }

        [Required]
        [MaxLength(50)]
        public string Code { get; private set; } = default!;

        [Required]
        [MaxLength(200)]
        public string Name { get; private set; } = default!;

        public Guid ClientId { get; private set; }
        public Guid ManagerId { get; private set; }

        public ProjectStatus Status { get; private set; }
        public ProjectType Type { get; private set; }
        public RiskLevel RiskLevel { get; private set; }

        public DateRange Period { get; private set; } = default!;
        public Money Budget
        {
            get
            {
                // Currency가 null이거나 빈 문자열인 경우 USD로 새 Money 객체 생성
                if (_budget == null || string.IsNullOrEmpty(_budget.Currency))
                {
                    return new Money(_budget?.Amount ?? 0, "USD");
                }
                return _budget;
            }
            private set
            {
                _budget = value ?? Money.Zero("USD");
            }
        }

        [Range(0, 100)]
        public decimal ProfitMargin { get; private set; }

        public string? Description { get; private set; }

        public IReadOnlyCollection<ProjectAssignment> Assignments => _assignments.AsReadOnly();
        public IReadOnlyCollection<ProjectMilestone> Milestones => _milestones.AsReadOnly();
        public IReadOnlyCollection<string> Technologies => _technologies?.AsReadOnly() ?? new List<string>().AsReadOnly();

        // Navigation properties
        public Client Client { get; private set; } = default!;
        public Employee Manager { get; private set; } = default!;

        public void UpdateBasicInfo(string name, string? description)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));

            Name = name;
            Description = description;
            UpdateTimestamp();
        }

        public void UpdateBudget(Money budget, decimal profitMargin)
        {
            Guard.AgainstNull(budget, nameof(budget));
            Guard.Against(profitMargin < 0 || profitMargin > 100,
                "Profit margin must be between 0 and 100");

            Budget = budget;
            ProfitMargin = profitMargin;
            UpdateTimestamp();
        }

        public void ChangeStatus(ProjectStatus newStatus)
        {
            ValidateStatusTransition(Status, newStatus);
            Status = newStatus;
            UpdateTimestamp();
        }

        public void AssignEmployee(Guid employeeId, string role, int allocationPercentage,
            DateRange assignmentPeriod, decimal hourlyRate = 0)
        {
            Guard.AgainstInvalidGuid(employeeId, nameof(employeeId));
            Guard.AgainstNullOrEmpty(role, nameof(role));
            Guard.Against(allocationPercentage <= 0 || allocationPercentage > 100,
                "Allocation percentage must be between 1 and 100");
            Guard.AgainstNull(assignmentPeriod, nameof(assignmentPeriod));

            var existingAssignment = _assignments.FirstOrDefault(a =>
                a.EmployeeId == employeeId && a.Period.Overlaps(assignmentPeriod));

            if (existingAssignment != null)
            {
                throw new DomainException("Employee is already assigned to this project during the specified period");
            }

            var assignment = new ProjectAssignment(Id, employeeId, role, allocationPercentage,
                assignmentPeriod, hourlyRate);
            _assignments.Add(assignment);

            AddDomainEvent(new EmployeeAssignedToProjectEvent(Id, employeeId, role,
                allocationPercentage, TenantId));
            UpdateTimestamp();
        }

        public void AddMilestone(string name, DateTime dueDate, string? description = null,
            decimal paymentPercentage = 0)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.Against(dueDate < Period.StartDate, "Milestone due date cannot be before project start");
            Guard.Against(Period.EndDate.HasValue && dueDate > Period.EndDate,
                "Milestone due date cannot be after project end");

            var milestone = new ProjectMilestone(Id, name, dueDate, description, paymentPercentage);
            _milestones.Add(milestone);
            UpdateTimestamp();
        }

        public void AddTechnology(string technology)
        {
            Guard.AgainstNullOrEmpty(technology, nameof(technology));

            if (!_technologies.Contains(technology, StringComparer.OrdinalIgnoreCase))
            {
                _technologies.Add(technology);
                UpdateTimestamp();
            }
        }

        public void RemoveTechnology(string technology)
        {
            Guard.AgainstNullOrEmpty(technology, nameof(technology));

            var existing = _technologies.FirstOrDefault(t =>
                string.Equals(t, technology, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                _technologies.Remove(existing);
                UpdateTimestamp();
            }
        }

        public Money CalculateEstimatedCost()
        {
            var totalCost = _assignments
                .Where(a => a.HourlyRate > 0)
                .Sum(a => a.CalculateEstimatedCost());

            // Budget의 Currency가 항상 USD가 되도록 보장
            return new Money(totalCost, "USD");
        }

        public decimal CalculateProgress()
        {
            if (!_milestones.Any()) return 0;

            var completedMilestones = _milestones.Count(m => m.IsCompleted);
            return (decimal)completedMilestones / _milestones.Count * 100;
        }

        public bool IsOverBudget()
        {
            var estimatedCost = CalculateEstimatedCost();
            return estimatedCost.Amount > Budget.Amount;
        }

        private static void ValidateStatusTransition(ProjectStatus from, ProjectStatus to)
        {
            var validTransitions = new Dictionary<ProjectStatus, ProjectStatus[]>
            {
                [ProjectStatus.Planning] = new[] { ProjectStatus.Active, ProjectStatus.Cancelled },
                [ProjectStatus.Active] = new[] { ProjectStatus.OnHold, ProjectStatus.Completed, ProjectStatus.Cancelled },
                [ProjectStatus.OnHold] = new[] { ProjectStatus.Active, ProjectStatus.Cancelled },
                [ProjectStatus.Completed] = Array.Empty<ProjectStatus>(),
                [ProjectStatus.Cancelled] = Array.Empty<ProjectStatus>()
            };

            if (!validTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
            {
                throw new BusinessRuleViolationException(
                    "INVALID_STATUS_TRANSITION",
                    $"Cannot change project status from {from} to {to}");
            }
        }
    }
}