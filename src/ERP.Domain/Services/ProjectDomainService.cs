using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Services
{
    /// <summary>
    /// 프로젝트 관련 복잡한 비즈니스 로직을 처리하는 도메인 서비스
    /// </summary>
    public class ProjectDomainService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;

        public ProjectDomainService(
            IProjectRepository projectRepository,
            IEmployeeRepository employeeRepository,
            ITimeEntryRepository timeEntryRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _timeEntryRepository = timeEntryRepository ?? throw new ArgumentNullException(nameof(timeEntryRepository));
        }

        /// <summary>
        /// 직원을 프로젝트에 할당할 수 있는지 확인
        /// </summary>
        public async Task<Result<bool>> CanAssignEmployeeToProjectAsync(
            Guid employeeId,
            Guid projectId,
            DateRange period,
            int allocationPercentage,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Guard.AgainstInvalidGuid(employeeId, nameof(employeeId));
                Guard.AgainstInvalidGuid(projectId, nameof(projectId));
                Guard.AgainstNull(period, nameof(period));
                Guard.Against(allocationPercentage <= 0 || allocationPercentage > 100,
                    "Allocation percentage must be between 1 and 100");

                var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<bool>("Employee not found");
                }

                var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
                if (project == null)
                {
                    return Result.Failure<bool>("Project not found");
                }

                // 직원이 활성 상태인지 확인
                if (employee.Status != EmployeeStatus.Active)
                {
                    return Result.Failure<bool>("Employee is not active");
                }

                // 프로젝트가 할당 가능한 상태인지 확인
                if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled)
                {
                    return Result.Failure<bool>("Cannot assign to completed or cancelled project");
                }

                // 기간이 프로젝트 기간과 겹치는지 확인
                if (!project.Period.Overlaps(period))
                {
                    return Result.Failure<bool>("Assignment period does not overlap with project period");
                }

                // 직원의 가용 할당률 확인
                var availableAllocation = employee.GetAvailableAllocation(period.StartDate, period.EndDate);
                if (availableAllocation < allocationPercentage)
                {
                    return Result.Failure<bool>($"Employee only has {availableAllocation}% allocation available");
                }

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error checking employee assignment: {ex.Message}");
            }
        }

        /// <summary>
        /// 고유한 프로젝트 코드 생성
        /// </summary>
        public async Task<Result<ProjectCode>> GenerateUniqueProjectCodeAsync(
            string prefix,
            DateTime? date = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Guard.AgainstNullOrEmpty(prefix, nameof(prefix));

                var targetDate = date ?? DateTime.Today;
                var baseCode = ProjectCode.Generate(prefix, targetDate);
                var counter = 1;
                var code = baseCode;

                // 중복되지 않는 코드를 찾을 때까지 반복
                while (await _projectRepository.ExistsByCodeAsync(code, cancellationToken))
                {
                    code = new ProjectCode($"{prefix.ToUpper()}-{targetDate.Year}-{targetDate.Month:D2}-{counter:D2}");
                    counter++;

                    if (counter > 99)
                    {
                        return Result.Failure<ProjectCode>("Cannot generate unique project code. Too many projects for this month.");
                    }
                }

                return Result.Success(code);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectCode>($"Error generating project code: {ex.Message}");
            }
        }

        /// <summary>
        /// 프로젝트 수익성 계산
        /// </summary>
        public async Task<Result<ProjectProfitabilityResult>> CalculateProjectProfitabilityAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Guard.AgainstInvalidGuid(projectId, nameof(projectId));

                var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
                if (project == null)
                {
                    return Result.Failure<ProjectProfitabilityResult>("Project not found");
                }

                // 실제 비용 계산 (시간 기록 기반)
                var actualCosts = await CalculateActualProjectCostsAsync(projectId, cancellationToken);

                // 예상 비용 계산 (할당 기반)
                var estimatedCosts = project.CalculateEstimatedCost();

                // 수익성 계산
                var result = new ProjectProfitabilityResult
                {
                    ProjectId = projectId,
                    Budget = project.Budget,
                    EstimatedCosts = estimatedCosts,
                    ActualCosts = new Money(actualCosts, project.Budget.Currency),
                    ProfitMargin = project.ProfitMargin,
                    EstimatedProfit = project.Budget.Subtract(estimatedCosts),
                    ActualProfit = project.Budget.Subtract(new Money(actualCosts, project.Budget.Currency)),
                    ProfitabilityPercentage = CalculateProfitabilityPercentage(project.Budget.Amount, actualCosts),
                    IsOverBudget = actualCosts > project.Budget.Amount,
                    BudgetUtilization = project.Budget.Amount > 0 ? (actualCosts / project.Budget.Amount) * 100 : 0
                };

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectProfitabilityResult>($"Error calculating profitability: {ex.Message}");
            }
        }

        /// <summary>
        /// 프로젝트의 리소스 활용도 분석
        /// </summary>
        public async Task<Result<ProjectResourceUtilizationResult>> AnalyzeProjectResourceUtilizationAsync(
            Guid projectId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Guard.AgainstInvalidGuid(projectId, nameof(projectId));

                var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
                if (project == null)
                {
                    return Result.Failure<ProjectResourceUtilizationResult>("Project not found");
                }

                var analysisStartDate = startDate ?? project.Period.StartDate;
                var analysisEndDate = endDate ?? project.Period.EndDate ?? DateTime.Today;

                // 할당된 직원들의 시간 기록 분석
                var employeeUtilizations = new List<EmployeeUtilization>();

                foreach (var assignment in project.Assignments)
                {
                    var employee = assignment.Employee;
                    var totalHours = await _timeEntryRepository.GetTotalHoursByEmployeeAsync(
                        employee.Id, analysisStartDate, analysisEndDate, false, cancellationToken);

                    var billableHours = await _timeEntryRepository.GetTotalHoursByEmployeeAsync(
                        employee.Id, analysisStartDate, analysisEndDate, true, cancellationToken);

                    var workingDays = CalculateWorkingDays(analysisStartDate, analysisEndDate);
                    var expectedHours = workingDays * 8 * assignment.AllocationPercentage / 100;

                    employeeUtilizations.Add(new EmployeeUtilization
                    {
                        EmployeeId = employee.Id,
                        EmployeeName = employee.Name,
                        Role = assignment.Role,
                        AllocationPercentage = assignment.AllocationPercentage,
                        ExpectedHours = expectedHours,
                        ActualHours = totalHours,
                        BillableHours = billableHours,
                        UtilizationRate = expectedHours > 0 ? (totalHours / expectedHours) * 100 : 0,
                        BillableRate = totalHours > 0 ? (billableHours / totalHours) * 100 : 0
                    });
                }

                var result = new ProjectResourceUtilizationResult
                {
                    ProjectId = projectId,
                    AnalysisPeriod = new DateRange(analysisStartDate, analysisEndDate),
                    EmployeeUtilizations = employeeUtilizations,
                    AverageUtilizationRate = employeeUtilizations.Any() ? employeeUtilizations.Average(e => e.UtilizationRate) : 0,
                    AverageBillableRate = employeeUtilizations.Any() ? employeeUtilizations.Average(e => e.BillableRate) : 0,
                    TotalExpectedHours = employeeUtilizations.Sum(e => e.ExpectedHours),
                    TotalActualHours = employeeUtilizations.Sum(e => e.ActualHours),
                    TotalBillableHours = employeeUtilizations.Sum(e => e.BillableHours)
                };

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectResourceUtilizationResult>($"Error analyzing resource utilization: {ex.Message}");
            }
        }

        /// <summary>
        /// 프로젝트 완료 가능성 평가
        /// </summary>
        public Result<ProjectCompletionAssessment> AssessProjectCompletion(Project project)
        {
            try
            {
                Guard.AgainstNull(project, nameof(project));

                var assessment = new ProjectCompletionAssessment
                {
                    ProjectId = project.Id,
                    CurrentStatus = project.Status,
                    Progress = project.CalculateProgress(),
                    IsOnSchedule = IsProjectOnSchedule(project),
                    IsWithinBudget = !project.IsOverBudget(),
                    RiskLevel = project.RiskLevel,
                    CompletedMilestones = project.Milestones.Count(m => m.IsCompleted),
                    TotalMilestones = project.Milestones.Count,
                    OverdueMilestones = project.Milestones.Count(m => m.IsOverdue),
                    Recommendations = GenerateProjectRecommendations(project)
                };

                // 완료 가능성 점수 계산 (0-100)
                assessment.CompletionProbability = CalculateCompletionProbability(project, assessment);

                return Result.Success(assessment);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectCompletionAssessment>($"Error assessing project completion: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private async Task<decimal> CalculateActualProjectCostsAsync(Guid projectId, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
            if (project == null) return 0;

            decimal totalCost = 0;

            foreach (var assignment in project.Assignments)
            {
                var totalHours = await _timeEntryRepository.GetTotalHoursByEmployeeAsync(
                    assignment.EmployeeId,
                    assignment.Period.StartDate,
                    assignment.Period.EndDate ?? DateTime.MinValue,
                    true, // billable only
                    cancellationToken);

                totalCost += totalHours * assignment.HourlyRate;
            }

            return totalCost;
        }

        private static decimal CalculateProfitabilityPercentage(decimal budget, decimal actualCosts)
        {
            if (budget == 0) return 0;
            var profit = budget - actualCosts;
            return (profit / budget) * 100;
        }

        private static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            var days = 0;
            var current = startDate.Date;

            while (current <= endDate.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
                current = current.AddDays(1);
            }

            return days;
        }

        private static bool IsProjectOnSchedule(Project project)
        {
            if (!project.Period.EndDate.HasValue) return true;

            var progress = project.CalculateProgress();
            var timeElapsed = (DateTime.Today - project.Period.StartDate).Days;
            var totalTime = (project.Period.EndDate.Value - project.Period.StartDate).Days;

            if (totalTime == 0) return true;

            var expectedProgress = (decimal)timeElapsed / totalTime * 100;
            return progress >= expectedProgress - 10; // 10% tolerance
        }

        private static List<string> GenerateProjectRecommendations(Project project)
        {
            var recommendations = new List<string>();

            if (project.IsOverBudget())
            {
                recommendations.Add("Project is over budget. Consider cost optimization measures.");
            }

            var overdueMilestones = project.Milestones.Count(m => m.IsOverdue);
            if (overdueMilestones > 0)
            {
                recommendations.Add($"There are {overdueMilestones} overdue milestones. Review project timeline.");
            }

            if (project.RiskLevel == RiskLevel.High)
            {
                recommendations.Add("High risk project requires increased monitoring and risk mitigation.");
            }

            if (!IsProjectOnSchedule(project))
            {
                recommendations.Add("Project appears to be behind schedule. Consider resource reallocation.");
            }

            return recommendations;
        }

        private static decimal CalculateCompletionProbability(Project project, ProjectCompletionAssessment assessment)
        {
            decimal probability = 50; // Base probability

            // Adjust based on progress
            probability += assessment.Progress * 0.3m;

            // Adjust based on schedule adherence
            if (assessment.IsOnSchedule) probability += 20;
            else probability -= 15;

            // Adjust based on budget adherence
            if (assessment.IsWithinBudget) probability += 15;
            else probability -= 20;

            // Adjust based on risk level
            probability -= (int)project.RiskLevel * 5;

            // Adjust based on milestone completion rate
            if (assessment.TotalMilestones > 0)
            {
                var milestoneCompletionRate = (decimal)assessment.CompletedMilestones / assessment.TotalMilestones;
                probability += milestoneCompletionRate * 15;
            }

            // Adjust based on overdue milestones
            probability -= assessment.OverdueMilestones * 5;

            return Math.Max(0, Math.Min(100, probability));
        }

        #endregion
    }

    #region Result Classes

    public class ProjectProfitabilityResult
    {
        public Guid ProjectId { get; set; }
        public Money Budget { get; set; } = default!;
        public Money EstimatedCosts { get; set; } = default!;
        public Money ActualCosts { get; set; } = default!;
        public decimal ProfitMargin { get; set; }
        public Money EstimatedProfit { get; set; } = default!;
        public Money ActualProfit { get; set; } = default!;
        public decimal ProfitabilityPercentage { get; set; }
        public bool IsOverBudget { get; set; }
        public decimal BudgetUtilization { get; set; }
    }

    public class ProjectResourceUtilizationResult
    {
        public Guid ProjectId { get; set; }
        public DateRange AnalysisPeriod { get; set; } = default!;
        public List<EmployeeUtilization> EmployeeUtilizations { get; set; } = new();
        public decimal AverageUtilizationRate { get; set; }
        public decimal AverageBillableRate { get; set; }
        public decimal TotalExpectedHours { get; set; }
        public decimal TotalActualHours { get; set; }
        public decimal TotalBillableHours { get; set; }
    }

    public class EmployeeUtilization
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public int AllocationPercentage { get; set; }
        public decimal ExpectedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal BillableHours { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal BillableRate { get; set; }
    }

    public class ProjectCompletionAssessment
    {
        public Guid ProjectId { get; set; }
        public ProjectStatus CurrentStatus { get; set; }
        public decimal Progress { get; set; }
        public bool IsOnSchedule { get; set; }
        public bool IsWithinBudget { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public int CompletedMilestones { get; set; }
        public int TotalMilestones { get; set; }
        public int OverdueMilestones { get; set; }
        public decimal CompletionProbability { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    #endregion
}
