using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

namespace ERP.Application.Services
{
    public class EmployeeApplicationService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IMapper _mapper;

        public EmployeeApplicationService(
            IEmployeeRepository employeeRepository,
            ITimeEntryRepository timeEntryRepository,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _timeEntryRepository = timeEntryRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<EmployeeDto>>> GetAvailableEmployeesAsync(
            DateTime startDate, DateTime? endDate = null, string? requiredSkill = null,
            SkillLevel? minSkillLevel = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IEnumerable<Domain.Entities.Employee> employees;

                if (!string.IsNullOrEmpty(requiredSkill))
                {
                    employees = await _employeeRepository.GetBySkillAsync(requiredSkill, minSkillLevel, cancellationToken);
                }
                else
                {
                    employees = await _employeeRepository.GetAvailableEmployeesAsync(startDate, endDate, cancellationToken);
                }

                // 추가 필터링: 실제로 해당 기간에 사용 가능한 직원들만
                var availableEmployees = employees.Where(e => e.IsAvailableForProject(startDate, endDate)).ToList();

                var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(availableEmployees);
                return Result.Success(employeeDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<EmployeeDto>>($"Error retrieving available employees: {ex.Message}");
            }
        }

        public async Task<Result<EmployeePerformanceDto>> GetEmployeePerformanceAsync(
            Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<EmployeePerformanceDto>("Employee not found");
                }

                var totalHours = await _timeEntryRepository.GetTotalHoursByEmployeeAsync(
                    employeeId, startDate, endDate, false, cancellationToken);

                var billableHours = await _timeEntryRepository.GetTotalHoursByEmployeeAsync(
                    employeeId, startDate, endDate, true, cancellationToken);

                var averageHours = await _timeEntryRepository.GetAverageHoursPerDayAsync(
                    employeeId, startDate, endDate, cancellationToken);

                var workingDays = CalculateWorkingDays(startDate, endDate);
                var expectedHours = workingDays * 8; // 8 hours per day

                var performance = new EmployeePerformanceDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.Name,
                    Period = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                    TotalHours = totalHours,
                    BillableHours = billableHours,
                    NonBillableHours = totalHours - billableHours,
                    ExpectedHours = expectedHours,
                    AverageHoursPerDay = averageHours,
                    UtilizationRate = expectedHours > 0 ? (totalHours / expectedHours) * 100 : 0,
                    BillableRate = totalHours > 0 ? (billableHours / totalHours) * 100 : 0,
                    EfficiencyScore = CalculateEfficiencyScore(totalHours, billableHours, expectedHours)
                };

                return Result.Success(performance);
            }
            catch (Exception ex)
            {
                return Result.Failure<EmployeePerformanceDto>($"Error calculating performance: {ex.Message}");
            }
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

        private static decimal CalculateEfficiencyScore(decimal totalHours, decimal billableHours, decimal expectedHours)
        {
            if (expectedHours == 0) return 0;

            var utilizationScore = Math.Min(totalHours / expectedHours, 1.2m) * 50; // Max 60 points
            var billabilityScore = totalHours > 0 ? (billableHours / totalHours) * 40 : 0; // Max 40 points

            return Math.Min(utilizationScore + billabilityScore, 100);
        }
    }

    // Supporting DTOs
    public class EmployeePerformanceDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = default!;
        public DateRangeDto Period { get; set; } = default!;
        public decimal TotalHours { get; set; }
        public decimal BillableHours { get; set; }
        public decimal NonBillableHours { get; set; }
        public decimal ExpectedHours { get; set; }
        public decimal AverageHoursPerDay { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal BillableRate { get; set; }
        public decimal EfficiencyScore { get; set; }
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}