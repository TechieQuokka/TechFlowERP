using System.ComponentModel.DataAnnotations;

using ERP.Domain.Enums;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class Employee : BaseEntity
    {
        private readonly List<EmployeeSkill> _skills = new();
        private readonly List<ProjectAssignment> _assignments = new();

        private Employee() { } // EF Core

        public Employee(string tenantId, string name, string email, DateTime hireDate)
            : base(tenantId)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(email, nameof(email));

            Name = name;
            Email = email;
            HireDate = hireDate.Date;
            Status = EmployeeStatus.Active;
            LeaveBalance = 0; // 초기 휴가 잔여일수
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; private set; } = default!;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; private set; } = default!;

        [Required]
        public DateTime HireDate { get; private set; }

        [Required]
        public EmployeeStatus Status { get; private set; }

        public Guid? DepartmentId { get; private set; }

        public Guid? ManagerId { get; private set; }

        [MaxLength(100)]
        public string? Position { get; private set; }

        [Range(0, double.MaxValue)]
        public decimal? Salary { get; private set; }

        [Range(0, int.MaxValue)]
        public int LeaveBalance { get; private set; }

        public IReadOnlyCollection<EmployeeSkill> Skills => _skills.AsReadOnly();
        public IReadOnlyCollection<ProjectAssignment> Assignments => _assignments.AsReadOnly();

        // Navigation properties
        public Employee? Manager { get; private set; }
        public List<Employee> Subordinates { get; private set; } = new();

        #region Public Methods

        /// <summary>
        /// 직원 기본 정보 업데이트
        /// </summary>
        public void UpdatePersonalInfo(string name, string email)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(email, nameof(email));
            Guard.Against(Status == EmployeeStatus.Terminated, "Cannot update terminated employee information");

            Name = name;
            Email = email;
            UpdateTimestamp();
        }

        /// <summary>
        /// 직책 및 급여 정보 업데이트
        /// </summary>
        public void UpdatePosition(string? position, decimal? salary = null)
        {
            Guard.Against(Status == EmployeeStatus.Terminated, "Cannot update terminated employee position");

            Position = position;
            if (salary.HasValue)
            {
                Guard.AgainstNegative(salary.Value, nameof(salary));
                Salary = salary;
            }
            UpdateTimestamp();
        }

        /// <summary>
        /// 부서 및 매니저 할당
        /// </summary>
        public void AssignToDepartment(Guid departmentId, Guid? managerId = null)
        {
            Guard.Against(Status == EmployeeStatus.Terminated, "Cannot assign terminated employee to department");
            Guard.AgainstInvalidGuid(departmentId, nameof(departmentId));

            // 자기 자신을 매니저로 할당하는 것 방지
            if (managerId.HasValue)
            {
                Guard.Against(managerId.Value == Id, "Employee cannot be their own manager");
            }

            DepartmentId = departmentId;
            ManagerId = managerId;
            UpdateTimestamp();
        }

        /// <summary>
        /// 기술 스킬 추가 또는 업데이트
        /// </summary>
        public void AddOrUpdateSkill(string technology, SkillLevel level, int yearsExperience, string? certification = null)
        {
            Guard.AgainstNullOrEmpty(technology, nameof(technology));
            Guard.AgainstNegative(yearsExperience, nameof(yearsExperience));
            Guard.Against(Status == EmployeeStatus.Terminated, "Cannot update skills for terminated employee");

            var existingSkill = _skills.FirstOrDefault(s =>
                string.Equals(s.Technology, technology, StringComparison.OrdinalIgnoreCase));

            if (existingSkill != null)
            {
                existingSkill.UpdateSkill(level, yearsExperience);
                if (!string.IsNullOrEmpty(certification))
                {
                    existingSkill.AddCertification(certification);
                }
            }
            else
            {
                var skill = new EmployeeSkill(Id, technology, level, yearsExperience);
                if (!string.IsNullOrEmpty(certification))
                {
                    skill.AddCertification(certification);
                }
                _skills.Add(skill);
            }
            UpdateTimestamp();
        }

        /// <summary>
        /// 특정 기술 스킬 제거
        /// </summary>
        public void RemoveSkill(string technology)
        {
            Guard.AgainstNullOrEmpty(technology, nameof(technology));
            Guard.Against(Status == EmployeeStatus.Terminated, "Cannot update skills for terminated employee");

            var skill = _skills.FirstOrDefault(s =>
                string.Equals(s.Technology, technology, StringComparison.OrdinalIgnoreCase));

            if (skill != null)
            {
                _skills.Remove(skill);
                UpdateTimestamp();
            }
        }

        /// <summary>
        /// 직원 상태 변경
        /// </summary>
        public void ChangeStatus(EmployeeStatus newStatus)
        {
            ValidateStatusTransition(Status, newStatus);

            var oldStatus = Status;
            Status = newStatus;

            // 상태 변경에 따른 추가 처리
            if (newStatus == EmployeeStatus.Terminated)
            {
                // 종료 시 모든 활성 프로젝트 할당 해제 (비즈니스 로직)
                HandleTermination();
            }
            else if (oldStatus == EmployeeStatus.Inactive && newStatus == EmployeeStatus.Active)
            {
                // 재활성화 시 처리
                HandleReactivation();
            }

            UpdateTimestamp();
        }

        /// <summary>
        /// 휴가 잔여일수 업데이트
        /// </summary>
        public void UpdateLeaveBalance(int newBalance)
        {
            Guard.AgainstNegative(newBalance, nameof(newBalance));
            Guard.Against(Status == EmployeeStatus.Terminated, "Cannot update leave balance for terminated employee");

            LeaveBalance = newBalance;
            UpdateTimestamp();
        }

        /// <summary>
        /// 휴가 사용
        /// </summary>
        public void UseLeave(int days)
        {
            Guard.Against(days <= 0, "Leave days must be positive");
            Guard.Against(Status != EmployeeStatus.Active, "Only active employees can use leave");
            Guard.Against(LeaveBalance < days, "Insufficient leave balance");

            LeaveBalance -= days;
            UpdateTimestamp();
        }

        /// <summary>
        /// 휴가 추가 (연차 지급 등)
        /// </summary>
        public void AddLeave(int days)
        {
            Guard.Against(days <= 0, "Leave days must be positive");

            LeaveBalance += days;
            UpdateTimestamp();
        }

        /// <summary>
        /// 특정 기간에 프로젝트 할당 가능 여부 확인
        /// </summary>
        public bool IsAvailableForProject(DateTime startDate, DateTime? endDate = null)
        {
            if (Status != EmployeeStatus.Active) return false;

            var period = new ValueObjects.DateRange(startDate, endDate);
            var conflictingAssignments = _assignments
                .Where(a => a.Period.Overlaps(period))
                .ToList();

            var totalAllocation = conflictingAssignments.Sum(a => a.AllocationPercentage);
            return totalAllocation < 100;
        }

        /// <summary>
        /// 특정 기간의 가용 할당률 계산
        /// </summary>
        public int GetAvailableAllocation(DateTime startDate, DateTime? endDate = null)
        {
            if (Status != EmployeeStatus.Active) return 0;

            var period = new ValueObjects.DateRange(startDate, endDate);
            var conflictingAssignments = _assignments
                .Where(a => a.Period.Overlaps(period))
                .ToList();

            var totalAllocation = conflictingAssignments.Sum(a => a.AllocationPercentage);
            return Math.Max(0, 100 - totalAllocation);
        }

        /// <summary>
        /// 특정 기술 보유 여부 확인
        /// </summary>
        public bool HasSkill(string technology, SkillLevel? minLevel = null)
        {
            Guard.AgainstNullOrEmpty(technology, nameof(technology));

            var skill = _skills.FirstOrDefault(s =>
                string.Equals(s.Technology, technology, StringComparison.OrdinalIgnoreCase));

            if (skill == null) return false;

            return minLevel == null || skill.Level >= minLevel;
        }

        /// <summary>
        /// 근속년수 계산
        /// </summary>
        public int GetYearsOfService()
        {
            var endDate = Status == EmployeeStatus.Terminated ? UpdatedAt : DateTime.Today;
            return (endDate - HireDate).Days / 365;
        }

        /// <summary>
        /// 현재 활성 프로젝트 할당 목록
        /// </summary>
        public IEnumerable<ProjectAssignment> GetActiveAssignments()
        {
            return _assignments.Where(a => a.IsActiveOn(DateTime.Today));
        }

        /// <summary>
        /// 현재 총 할당률
        /// </summary>
        public int GetCurrentTotalAllocation()
        {
            return GetActiveAssignments().Sum(a => a.AllocationPercentage);
        }

        #endregion

        #region Private Helper Methods

        private static void ValidateStatusTransition(EmployeeStatus from, EmployeeStatus to)
        {
            var validTransitions = new Dictionary<EmployeeStatus, EmployeeStatus[]>
            {
                [EmployeeStatus.Active] = new[] { EmployeeStatus.Inactive, EmployeeStatus.OnLeave, EmployeeStatus.Terminated },
                [EmployeeStatus.Inactive] = new[] { EmployeeStatus.Active, EmployeeStatus.Terminated },
                [EmployeeStatus.OnLeave] = new[] { EmployeeStatus.Active, EmployeeStatus.Terminated },
                [EmployeeStatus.Terminated] = Array.Empty<EmployeeStatus>() // 종료된 직원은 상태 변경 불가
            };

            if (!validTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
            {
                throw new BusinessRuleViolationException(
                    "INVALID_EMPLOYEE_STATUS_TRANSITION",
                    $"Cannot change employee status from {from} to {to}");
            }
        }

        private void HandleTermination()
        {
            // 모든 활성 할당의 종료일을 오늘로 설정
            var activeAssignments = _assignments.Where(a => a.IsActiveOn(DateTime.Today)).ToList();
            foreach (var assignment in activeAssignments)
            {
                // 실제로는 Assignment 엔티티에 EndDate 설정 메서드가 필요
                // assignment.EndAssignment(DateTime.Today);
            }
        }

        private void HandleReactivation()
        {
            // 재활성화 시 필요한 비즈니스 로직
            // 예: 알림 발송, 로그 기록 등
        }

        #endregion

        #region Computed Properties

        /// <summary>
        /// 주요 기술 스킬 목록 (Expert/Advanced 레벨)
        /// </summary>
        public IEnumerable<string> PrimarySkills => _skills
            .Where(s => s.Level >= SkillLevel.Advanced)
            .Select(s => s.Technology)
            .ToList();

        /// <summary>
        /// 현재 과부하 상태 여부 (할당률 100% 초과)
        /// </summary>
        public bool IsOverallocated => GetCurrentTotalAllocation() > 100;

        /// <summary>
        /// 프로젝트 할당 가능 여부
        /// </summary>
        public bool IsAssignable => Status == EmployeeStatus.Active && !IsOverallocated;

        #endregion
    }
}