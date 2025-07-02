using System.ComponentModel.DataAnnotations;

using ERP.Domain.Enums;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class EmployeeSkill
    {
        private EmployeeSkill() { } // EF Core

        public EmployeeSkill(Guid employeeId, string technology, SkillLevel level, int yearsExperience)
        {
            Guard.AgainstInvalidGuid(employeeId, nameof(employeeId));
            Guard.AgainstNullOrEmpty(technology, nameof(technology));
            Guard.AgainstNegative(yearsExperience, nameof(yearsExperience));

            Id = Guid.NewGuid();
            EmployeeId = employeeId;
            Technology = technology;
            Level = level;
            YearsExperience = yearsExperience;
            LastUsedDate = DateTime.Today;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }

        [Required]
        [MaxLength(50)]
        public string Technology { get; private set; } = default!;

        public SkillLevel Level { get; private set; }
        public int YearsExperience { get; private set; }
        public DateTime? LastUsedDate { get; private set; }

        [MaxLength(200)]
        public string? Certification { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public void UpdateSkill(SkillLevel level, int yearsExperience)
        {
            Guard.AgainstNegative(yearsExperience, nameof(yearsExperience));

            Level = level;
            YearsExperience = yearsExperience;
            LastUsedDate = DateTime.Today;
        }

        public void AddCertification(string certification)
        {
            Guard.AgainstNullOrEmpty(certification, nameof(certification));
            Certification = certification;
        }

        public void MarkAsUsed()
        {
            LastUsedDate = DateTime.Today;
        }
    }
}
