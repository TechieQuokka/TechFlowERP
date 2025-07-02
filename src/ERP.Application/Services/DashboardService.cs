using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

namespace ERP.Application.Services
{
    public class DashboardService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        // ❌ Invoice Repository 제거
        // private readonly IInvoiceRepository _invoiceRepository;

        public DashboardService(
            IProjectRepository projectRepository,
            IEmployeeRepository employeeRepository,
            ITimeEntryRepository timeEntryRepository)
        // ❌ Invoice Repository 제거
        // IInvoiceRepository invoiceRepository)
        {
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
            _timeEntryRepository = timeEntryRepository;
            // _invoiceRepository = invoiceRepository;
        }

        public async Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = new DashboardSummaryDto();

                // Project metrics
                var allProjects = await _projectRepository.GetAllAsync(cancellationToken);
                summary.TotalProjects = allProjects.Count();
                summary.ActiveProjects = allProjects.Count(p => p.Status == ProjectStatus.Active);
                summary.CompletedProjects = allProjects.Count(p => p.Status == ProjectStatus.Completed);
                summary.ProjectsOnHold = allProjects.Count(p => p.Status == ProjectStatus.OnHold);

                // Employee metrics
                var allEmployees = await _employeeRepository.GetAllAsync(cancellationToken);
                summary.TotalEmployees = allEmployees.Count();
                summary.ActiveEmployees = allEmployees.Count(e => e.Status == EmployeeStatus.Active);

                // Revenue metrics
                var completedProjects = allProjects.Where(p => p.Status == ProjectStatus.Completed);
                summary.TotalRevenue = completedProjects.Sum(p => p.Budget.Amount);

                // Current month metrics
                var currentMonth = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
                var nextMonth = currentMonth.AddMonths(1);

                var currentMonthTimeEntries = await _timeEntryRepository.GetByDateRangeAsync(
                    currentMonth, nextMonth.AddDays(-1), cancellationToken);

                summary.CurrentMonthHours = currentMonthTimeEntries.Sum(te => te.Hours);
                summary.CurrentMonthBillableHours = currentMonthTimeEntries.Where(te => te.Billable).Sum(te => te.Hours);

                // ✅ Invoice 관련 코드 제거 또는 대체
                // Overdue invoices 대신 다른 지표 사용
                summary.OverdueInvoicesCount = 0;  // 임시로 0 설정
                summary.OverdueAmount = 0;         // 임시로 0 설정

                // 또는 다른 유용한 지표로 대체
                var pendingTimeEntries = await _timeEntryRepository.GetPendingApprovalAsync(null, cancellationToken);
                summary.PendingApprovalCount = pendingTimeEntries.Count();

                // Recent projects
                summary.RecentProjects = allProjects
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new RecentProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ClientName = p.Client.CompanyName,
                        Status = p.Status,
                        Progress = p.CalculateProgress()
                    })
                    .ToList();

                return Result.Success(summary);
            }
            catch (Exception ex)
            {
                return Result.Failure<DashboardSummaryDto>($"Error generating dashboard: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<ProjectStatusSummaryDto>>> GetProjectStatusSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var projects = await _projectRepository.GetAllAsync(cancellationToken);

                var statusSummary = projects
                    .GroupBy(p => p.Status)
                    .Select(g => new ProjectStatusSummaryDto
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalBudget = g.Sum(p => p.Budget.Amount),
                        AverageBudget = g.Average(p => p.Budget.Amount)
                    })
                    .ToList();

                return Result.Success<IEnumerable<ProjectStatusSummaryDto>>(statusSummary);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<ProjectStatusSummaryDto>>($"Error generating status summary: {ex.Message}");
            }
        }
    }

    // ✅ DashboardSummaryDto 수정
    public class DashboardSummaryDto
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int ProjectsOnHold { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CurrentMonthHours { get; set; }
        public decimal CurrentMonthBillableHours { get; set; }

        // ✅ Invoice 관련 필드 제거 또는 대체
        public int OverdueInvoicesCount { get; set; } = 0;    // 기본값 0
        public decimal OverdueAmount { get; set; } = 0;       // 기본값 0

        // ✅ 새로운 유용한 지표 추가
        public int PendingApprovalCount { get; set; }         // 승인 대기 시간 기록 수

        public List<RecentProjectDto> RecentProjects { get; set; } = new();
    }

    public class RecentProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string ClientName { get; set; } = default!;
        public ProjectStatus Status { get; set; }
        public decimal Progress { get; set; }
    }

    public class ProjectStatusSummaryDto
    {
        public ProjectStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal AverageBudget { get; set; }
    }
}