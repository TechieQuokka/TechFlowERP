using ERP.Domain.Entities;
using ERP.SharedKernel.Abstractions;

namespace ERP.Domain.Interfaces
{
    public interface ITimeEntryRepository : IRepository<TimeEntry>
    {
        // 기본 조회 메서드들
        Task<IEnumerable<TimeEntry>> GetByEmployeeIdAsync(Guid employeeId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TimeEntry>> GetByProjectIdAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TimeEntry>> GetPendingApprovalAsync(Guid? managerId = null, CancellationToken cancellationToken = default);

        // 집계 메서드들
        Task<decimal> GetTotalHoursByEmployeeAsync(Guid employeeId, DateTime startDate, DateTime endDate, bool billableOnly = false, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalHoursByProjectAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null, bool billableOnly = false, CancellationToken cancellationToken = default);

        // 누락된 메서드들 추가
        Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<TimeEntry>> GetUnapprovedByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<decimal> GetAverageHoursPerDayAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<DateTime, decimal>> GetDailyHoursSummaryAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        // 승인 관련
        Task<IEnumerable<TimeEntry>> GetApprovedByManagerAsync(Guid managerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<int> GetPendingApprovalCountAsync(Guid? managerId = null, CancellationToken cancellationToken = default);

        // 대량 작업
        Task ApproveMultipleAsync(IEnumerable<Guid> timeEntryIds, Guid approvedBy, CancellationToken cancellationToken = default);
        Task<IEnumerable<TimeEntry>> GetDuplicateEntriesAsync(Guid employeeId, Guid projectId, DateTime date, CancellationToken cancellationToken = default);

        // 통계 및 분석
        Task<decimal> GetTotalHoursByDateRangeAsync(DateTime startDate, DateTime endDate, bool billableOnly = false, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, decimal>> GetHoursByEmployeeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, decimal>> GetHoursByProjectAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        // 효율성 분석
        Task<decimal> GetUtilizationRateAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetBillableRateAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
