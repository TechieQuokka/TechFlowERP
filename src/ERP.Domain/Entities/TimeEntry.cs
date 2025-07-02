using System.ComponentModel.DataAnnotations;

using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    /// <summary>
    /// 시간 기록 엔티티 - Aggregate Root로 변경
    /// </summary>
    public class TimeEntry : BaseEntity
    {
        private TimeEntry() { } // EF Core

        public TimeEntry(string tenantId, Guid employeeId, Guid projectId, DateTime date,
            decimal hours, string? taskDescription = null, bool billable = true)
            : base(tenantId)
        {
            Guard.AgainstInvalidGuid(employeeId, nameof(employeeId));
            Guard.AgainstInvalidGuid(projectId, nameof(projectId));
            Guard.Against(hours <= 0 || hours > 24, "Hours must be between 0 and 24");
            Guard.Against(date.Date > DateTime.Today, "Cannot log time for future dates");

            EmployeeId = employeeId;
            ProjectId = projectId;
            Date = date.Date;
            Hours = hours;
            TaskDescription = taskDescription;
            Billable = billable;
            Approved = false;
        }

        public Guid EmployeeId { get; private set; }
        public Guid ProjectId { get; private set; }
        public DateTime Date { get; private set; }

        [Range(0.1, 24)]
        public decimal Hours { get; private set; }

        public string? TaskDescription { get; private set; }
        public bool Billable { get; private set; }
        public bool Approved { get; private set; }
        public Guid? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }

        // Navigation properties
        public Employee Employee { get; private set; } = default!;
        public Project Project { get; private set; } = default!;

        public void UpdateHours(decimal newHours)
        {
            Guard.Against(Approved, "Cannot update approved time entry");
            Guard.Against(newHours <= 0 || newHours > 24, "Hours must be between 0 and 24");

            Hours = newHours;
            UpdateTimestamp();
        }

        public void UpdateDescription(string? description)
        {
            Guard.Against(Approved, "Cannot update approved time entry");
            TaskDescription = description;
            UpdateTimestamp();
        }

        public void Approve(Guid approvedBy)
        {
            Guard.Against(Approved, "Time entry is already approved");
            Guard.AgainstInvalidGuid(approvedBy, nameof(approvedBy));

            Approved = true;
            ApprovedBy = approvedBy;
            ApprovedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void Reject()
        {
            Guard.Against(Approved, "Cannot reject approved time entry");

            Approved = false;
            ApprovedBy = null;
            ApprovedAt = null;
            UpdateTimestamp();
        }

        public void SetBillable(bool billable)
        {
            Guard.Against(Approved, "Cannot change billability of approved time entry");
            Billable = billable;
            UpdateTimestamp();
        }

        public decimal CalculateCost(decimal hourlyRate)
        {
            Guard.AgainstNegative(hourlyRate, nameof(hourlyRate));
            return Hours * hourlyRate;
        }

        public bool IsEditableBy(Guid userId)
        {
            // 승인되지 않았고, 본인이 작성했거나 매니저인 경우 수정 가능
            return !Approved && (EmployeeId == userId || ApprovedBy == userId);
        }
    }
}
