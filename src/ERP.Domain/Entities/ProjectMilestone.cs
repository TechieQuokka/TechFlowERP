using System.ComponentModel.DataAnnotations;

using ERP.SharedKernel.Utilities;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class ProjectMilestone
    {
        private ProjectMilestone() { } // EF Core

        public ProjectMilestone(Guid projectId, string name, DateTime dueDate,
            string? description = null, decimal paymentPercentage = 0)
        {
            Guard.AgainstInvalidGuid(projectId, nameof(projectId));
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.Against(paymentPercentage < 0 || paymentPercentage > 100,
                "Payment percentage must be between 0 and 100");

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Name = name;
            DueDate = dueDate.Date;
            Description = description;
            PaymentPercentage = paymentPercentage;
            Status = MilestoneStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; private set; } = default!;

        public string? Description { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime? CompletionDate { get; private set; }

        [Range(0, 100)]
        public decimal PaymentPercentage { get; private set; }

        public MilestoneStatus Status { get; private set; }
        public string? Deliverables { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Navigation property
        public Project Project { get; private set; } = default!;

        public bool IsCompleted => Status == MilestoneStatus.Completed;
        public bool IsOverdue => Status != MilestoneStatus.Completed && DateTime.Today > DueDate;
        public bool IsInProgress => Status == MilestoneStatus.InProgress;

        public int DaysUntilDue
        {
            get
            {
                if (IsCompleted) return 0;
                return (DueDate - DateTime.Today).Days;
            }
        }

        public int DaysOverdue
        {
            get
            {
                if (!IsOverdue) return 0;
                return (DateTime.Today - DueDate).Days;
            }
        }

        public void StartProgress()
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Cannot start progress on completed milestone");
            Guard.Against(Status == MilestoneStatus.InProgress, "Milestone is already in progress");

            Status = MilestoneStatus.InProgress;
        }

        public void MarkAsCompleted(string? deliverables = null)
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Milestone is already completed");

            Status = MilestoneStatus.Completed;
            CompletionDate = DateTime.UtcNow;
            Deliverables = deliverables;
        }

        public void MarkAsDelayed(string? reason = null)
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Cannot mark completed milestone as delayed");

            Status = MilestoneStatus.Delayed;
            if (!string.IsNullOrEmpty(reason))
            {
                Description = $"{Description}\nDelay Reason: {reason}";
            }
        }

        public void UpdateDescription(string? description)
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Cannot update description of completed milestone");
            Description = description;
        }

        public void UpdateDueDate(DateTime newDueDate)
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Cannot update due date of completed milestone");
            Guard.Against(newDueDate.Date <= DateTime.Today, "Due date must be in the future");

            DueDate = newDueDate.Date;

            // Reset status if it was delayed and new date is reasonable
            if (Status == MilestoneStatus.Delayed)
            {
                Status = MilestoneStatus.Pending;
            }
        }

        public void UpdatePaymentPercentage(decimal newPercentage)
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Cannot update payment percentage of completed milestone");
            Guard.Against(newPercentage < 0 || newPercentage > 100, "Payment percentage must be between 0 and 100");

            PaymentPercentage = newPercentage;
        }

        public void Reset()
        {
            Guard.Against(Status == MilestoneStatus.Completed, "Cannot reset completed milestone");

            Status = MilestoneStatus.Pending;
            CompletionDate = null;
            Deliverables = null;
        }

        public decimal CalculatePaymentAmount(decimal projectBudget)
        {
            Guard.AgainstNegative(projectBudget, nameof(projectBudget));
            return projectBudget * PaymentPercentage / 100;
        }

        public bool CanBeCompleted()
        {
            return Status == MilestoneStatus.InProgress || Status == MilestoneStatus.Pending;
        }

        public override string ToString()
        {
            return $"{Name} (Due: {DueDate:yyyy-MM-dd}, Status: {Status})";
        }
    }
}