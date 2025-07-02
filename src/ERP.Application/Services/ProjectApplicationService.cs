using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.Domain.Services;
using ERP.SharedKernel.Utilities;

namespace ERP.Application.Services
{
    public class ProjectApplicationService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ProjectDomainService _projectDomainService;
        private readonly IMapper _mapper;

        public ProjectApplicationService(
            IProjectRepository projectRepository,
            ProjectDomainService projectDomainService,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _projectDomainService = projectDomainService;
            _mapper = mapper;
        }

        public async Task<Result<ProjectProfitabilityDto>> GetProjectProfitabilityAsync(
            Guid projectId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _projectDomainService.CalculateProjectProfitabilityAsync(
                    projectId, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<ProjectProfitabilityDto>(result.Errors);
                }

                var dto = new ProjectProfitabilityDto
                {
                    ProjectId = result.Value.ProjectId,
                    Budget = result.Value.Budget.Amount,
                    Currency = result.Value.Budget.Currency,
                    EstimatedCosts = result.Value.EstimatedCosts.Amount,
                    ActualCosts = result.Value.ActualCosts.Amount,
                    ProfitMargin = result.Value.ProfitMargin,
                    EstimatedProfit = result.Value.EstimatedProfit.Amount,
                    ActualProfit = result.Value.ActualProfit.Amount,
                    ProfitabilityPercentage = result.Value.ProfitabilityPercentage,
                    IsOverBudget = result.Value.IsOverBudget,
                    BudgetUtilization = result.Value.BudgetUtilization
                };

                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectProfitabilityDto>($"Error calculating profitability: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<ProjectResourceUtilizationDto>>> GetResourceUtilizationAsync(
            Guid projectId, DateTime? startDate = null, DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _projectDomainService.AnalyzeProjectResourceUtilizationAsync(
                    projectId, startDate, endDate, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ProjectResourceUtilizationDto>>(result.Errors);
                }

                var dtos = result.Value.EmployeeUtilizations.Select(e => new ProjectResourceUtilizationDto
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.EmployeeName,
                    Role = e.Role,
                    AllocationPercentage = e.AllocationPercentage,
                    ExpectedHours = e.ExpectedHours,
                    ActualHours = e.ActualHours,
                    BillableHours = e.BillableHours,
                    UtilizationRate = e.UtilizationRate,
                    BillableRate = e.BillableRate
                });

                return Result.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<ProjectResourceUtilizationDto>>($"Error analyzing utilization: {ex.Message}");
            }
        }
    }

    // Supporting DTOs
    public class ProjectProfitabilityDto
    {
        public Guid ProjectId { get; set; }
        public decimal Budget { get; set; }
        public string Currency { get; set; } = default!;
        public decimal EstimatedCosts { get; set; }
        public decimal ActualCosts { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal EstimatedProfit { get; set; }
        public decimal ActualProfit { get; set; }
        public decimal ProfitabilityPercentage { get; set; }
        public bool IsOverBudget { get; set; }
        public decimal BudgetUtilization { get; set; }
    }

    public class ProjectResourceUtilizationDto
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
}