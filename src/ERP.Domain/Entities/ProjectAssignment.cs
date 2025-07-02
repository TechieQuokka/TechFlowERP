using System.ComponentModel.DataAnnotations;

using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class ProjectAssignment
    {
        private ProjectAssignment() { } // EF Core

        public ProjectAssignment(Guid projectId, Guid employeeId, string role,
            int allocationPercentage, DateRange period, decimal hourlyRate = 0)
        {
            Guard.AgainstInvalidGuid(projectId, nameof(projectId));
            Guard.AgainstInvalidGuid(employeeId, nameof(employeeId));
            Guard.AgainstNullOrEmpty(role, nameof(role));
            Guard.Against(allocationPercentage <= 0 || allocationPercentage > 100,
                "Allocation percentage must be between 1 and 100");
            Guard.AgainstNull(period, nameof(period));
            Guard.AgainstNegative(hourlyRate, nameof(hourlyRate));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            EmployeeId = employeeId;
            Role = role;
            AllocationPercentage = allocationPercentage;
            Period = period;
            HourlyRate = hourlyRate;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public Guid EmployeeId { get; private set; }

        [Required]
        [MaxLength(100)]
        public string Role { get; private set; } = default!;

        [Range(1, 100)]
        public int AllocationPercentage { get; private set; }

        public DateRange Period { get; private set; } = default!;

        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; private set; }

        public DateTime CreatedAt { get; private set; }

        // Navigation properties
        public Project Project { get; private set; } = default!;
        public Employee Employee { get; private set; } = default!;

        public bool IsActiveOn(DateTime date)
        {
            return Period.Contains(date);
        }

        public decimal CalculateEstimatedCost(int estimatedHours = 160) // 160 hours per month default
        {
            if (HourlyRate <= 0) return 0;

            var duration = Period.DurationInDays;
            var workingDays = duration * 5 / 7; // Assume 5 working days per week
            var estimatedTotalHours = workingDays * 8 * AllocationPercentage / 100; // 8 hours per day

            return estimatedTotalHours * HourlyRate;
        }

        public void UpdateAllocation(int newAllocationPercentage)
        {
            Guard.Against(newAllocationPercentage <= 0 || newAllocationPercentage > 100,
                "Allocation percentage must be between 1 and 100");

            AllocationPercentage = newAllocationPercentage;
        }

        public void UpdateRate(decimal newHourlyRate)
        {
            Guard.AgainstNegative(newHourlyRate, nameof(newHourlyRate));
            HourlyRate = newHourlyRate;
        }
    }
}