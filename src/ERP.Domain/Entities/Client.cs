using System.ComponentModel.DataAnnotations;

using ERP.Domain.Enums;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class Client : BaseEntity
    {
        private readonly List<Project> _projects = new();

        private Client() { } // EF Core

        public Client(string tenantId, string companyName, string? industry = null)
            : base(tenantId)
        {
            Guard.AgainstNullOrEmpty(companyName, nameof(companyName));

            CompanyName = companyName;
            Industry = industry;
            // CreatedAt은 BaseEntity에서 자동으로 설정되므로 제거
        }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; private set; } = default!;

        [MaxLength(100)]
        public string? Industry { get; private set; }

        [MaxLength(100)]
        public string? ContactPerson { get; private set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? ContactEmail { get; private set; }

        [MaxLength(50)]
        public string? ContactPhone { get; private set; }

        [MaxLength(500)]
        public string? Address { get; private set; }

        public decimal? ContractValue { get; private set; }

        [MaxLength(50)]
        public string? ClientSize { get; private set; } // Small, Medium, Large

        public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

        #region Public Methods

        /// <summary>
        /// 연락처 정보 업데이트
        /// </summary>
        public void UpdateContactInfo(string? contactPerson, string? contactEmail, string? contactPhone)
        {
            // 이메일 형식 검증
            if (!string.IsNullOrEmpty(contactEmail))
            {
                Guard.Against(!IsValidEmail(contactEmail), "Invalid email format");
            }

            ContactPerson = contactPerson;
            ContactEmail = contactEmail;
            ContactPhone = contactPhone;
            UpdateTimestamp();
        }

        /// <summary>
        /// 주소 정보 업데이트
        /// </summary>
        public void UpdateAddress(string? address)
        {
            Address = address;
            UpdateTimestamp();
        }

        /// <summary>
        /// 회사 기본 정보 업데이트
        /// </summary>
        public void UpdateBasicInfo(string companyName, string? industry, string? clientSize = null)
        {
            Guard.AgainstNullOrEmpty(companyName, nameof(companyName));

            CompanyName = companyName;
            Industry = industry;
            ClientSize = clientSize;
            UpdateTimestamp();
        }

        /// <summary>
        /// 계약 금액 설정
        /// </summary>
        public void SetContractValue(decimal? contractValue)
        {
            if (contractValue.HasValue)
            {
                Guard.AgainstNegative(contractValue.Value, nameof(contractValue));
            }

            ContractValue = contractValue;
            UpdateTimestamp();
        }

        /// <summary>
        /// 활성 프로젝트 보유 여부 확인
        /// </summary>
        public bool HasActiveProjects()
        {
            return _projects.Any(p => p.Status == ProjectStatus.Active ||
                                      p.Status == ProjectStatus.Planning);
        }

        /// <summary>
        /// 특정 상태의 프로젝트 개수
        /// </summary>
        public int GetProjectCountByStatus(ProjectStatus status)
        {
            return _projects.Count(p => p.Status == status);
        }

        /// <summary>
        /// 총 프로젝트 예산 계산
        /// </summary>
        public decimal GetTotalProjectBudget()
        {
            return _projects.Where(p => p.Status != ProjectStatus.Cancelled)
                           .Sum(p => p.Budget.Amount);
        }

        /// <summary>
        /// 완료된 프로젝트 수
        /// </summary>
        public int GetCompletedProjectsCount()
        {
            return _projects.Count(p => p.Status == ProjectStatus.Completed);
        }

        /// <summary>
        /// 진행 중인 프로젝트 목록
        /// </summary>
        public IEnumerable<Project> GetActiveProjects()
        {
            return _projects.Where(p => p.Status == ProjectStatus.Active ||
                                       p.Status == ProjectStatus.Planning);
        }

        /// <summary>
        /// 최근 프로젝트 가져오기
        /// </summary>
        public Project? GetLatestProject()
        {
            return _projects.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        }

        /// <summary>
        /// 클라이언트 관계 지속 기간 (첫 프로젝트부터)
        /// </summary>
        public int GetRelationshipDurationInDays()
        {
            var firstProject = _projects.OrderBy(p => p.CreatedAt).FirstOrDefault();
            if (firstProject == null) return 0;

            return (DateTime.Today - firstProject.CreatedAt.Date).Days;
        }

        /// <summary>
        /// 클라이언트 삭제 가능 여부 확인
        /// </summary>
        public bool CanBeDeleted()
        {
            // 활성 프로젝트가 있으면 삭제 불가
            return !HasActiveProjects();
        }

        /// <summary>
        /// 클라이언트 중요도 계산 (프로젝트 수와 예산 기반)
        /// </summary>
        public string GetClientImportance()
        {
            var projectCount = _projects.Count;
            var totalBudget = GetTotalProjectBudget();

            if (projectCount >= 5 || totalBudget >= 100000)
                return "High";
            else if (projectCount >= 2 || totalBudget >= 25000)
                return "Medium";
            else
                return "Low";
        }

        /// <summary>
        /// 연간 매출 기여도 계산
        /// </summary>
        public decimal GetAnnualRevenue(int year)
        {
            return _projects.Where(p => p.CreatedAt.Year == year &&
                                       p.Status == ProjectStatus.Completed)
                           .Sum(p => p.Budget.Amount);
        }

        #endregion

        #region Private Helper Methods

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Computed Properties

        /// <summary>
        /// 총 프로젝트 개수
        /// </summary>
        public int TotalProjectsCount => _projects.Count;

        /// <summary>
        /// 활성 프로젝트 개수
        /// </summary>
        public int ActiveProjectsCount => _projects.Count(p => p.Status == ProjectStatus.Active);

        /// <summary>
        /// 성공률 (완료된 프로젝트 비율)
        /// </summary>
        public decimal SuccessRate
        {
            get
            {
                var totalProjects = _projects.Count(p => p.Status != ProjectStatus.Planning);
                if (totalProjects == 0) return 0;

                var completedProjects = GetCompletedProjectsCount();
                return (decimal)completedProjects / totalProjects * 100;
            }
        }

        /// <summary>
        /// 평균 프로젝트 예산
        /// </summary>
        public decimal AverageProjectBudget
        {
            get
            {
                var projects = _projects.Where(p => p.Status != ProjectStatus.Cancelled).ToList();
                if (!projects.Any()) return 0;

                return projects.Average(p => p.Budget.Amount);
            }
        }

        /// <summary>
        /// VIP 고객 여부 (높은 중요도 또는 높은 계약 금액)
        /// </summary>
        public bool IsVipClient => GetClientImportance() == "High" ||
                                  (ContractValue.HasValue && ContractValue >= 50000);

        /// <summary>
        /// 신규 고객 여부 (6개월 이내 첫 프로젝트)
        /// </summary>
        public bool IsNewClient => GetRelationshipDurationInDays() <= 180;

        #endregion

        #region ToString Override

        public override string ToString()
        {
            return $"{CompanyName} ({Industry ?? "Unknown Industry"}) - {ActiveProjectsCount} active projects";
        }

        #endregion
    }
}