using ERP.Domain.Entities;
using ERP.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories
{
    public class TimeEntryRepository : BaseRepository<TimeEntry>, ITimeEntryRepository
    {
        public TimeEntryRepository(ErpDbContext context) : base(context) { }

        public override async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Employee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<TimeEntry>> GetByEmployeeIdAsync(Guid employeeId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(t => t.Project)
                .Where(t => t.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeEntry>> GetByProjectIdAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(t => t.Employee)
                .Where(t => t.ProjectId == projectId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeEntry>> GetPendingApprovalAsync(Guid? managerId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(t => t.Employee)
                .Include(t => t.Project)
                .Where(t => !t.Approved);

            if (managerId.HasValue)
            {
                query = query.Where(t => t.Project.ManagerId == managerId.Value);
            }

            return await query.OrderBy(t => t.Date).ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalHoursByEmployeeAsync(Guid employeeId, DateTime startDate, DateTime endDate, bool billableOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(t => t.EmployeeId == employeeId &&
                                         t.Date >= startDate &&
                                         t.Date <= endDate);

            if (billableOnly)
                query = query.Where(t => t.Billable);

            return await query.SumAsync(t => t.Hours, cancellationToken);
        }

        public async Task<decimal> GetTotalHoursByProjectAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null, bool billableOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(t => t.ProjectId == projectId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            if (billableOnly)
                query = query.Where(t => t.Billable);

            return await query.SumAsync(t => t.Hours, cancellationToken);
        }

        public async Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Employee)
                .Include(t => t.Project)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TimeEntry>> GetUnapprovedByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Project)
                .Where(t => t.EmployeeId == employeeId && !t.Approved)
                .OrderByDescending(t => t.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetAverageHoursPerDayAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var dailyHours = await _dbSet
                .Where(t => t.EmployeeId == employeeId && t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => t.Date)
                .Select(g => g.Sum(t => t.Hours))
                .ToListAsync(cancellationToken);

            return dailyHours.Any() ? dailyHours.Average() : 0;
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyHoursSummaryAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.EmployeeId == employeeId && t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => t.Date)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Hours), cancellationToken);
        }

        public async Task<IEnumerable<TimeEntry>> GetApprovedByManagerAsync(Guid managerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(t => t.Employee)
                .Include(t => t.Project)
                .Where(t => t.Approved && t.ApprovedBy == managerId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync(cancellationToken);
        }

        public async Task<int> GetPendingApprovalCountAsync(Guid? managerId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(t => !t.Approved);

            if (managerId.HasValue)
            {
                query = query.Where(t => t.Project.ManagerId == managerId.Value);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task ApproveMultipleAsync(IEnumerable<Guid> timeEntryIds, Guid approvedBy, CancellationToken cancellationToken = default)
        {
            var timeEntries = await _dbSet
                .Where(t => timeEntryIds.Contains(t.Id) && !t.Approved)
                .ToListAsync(cancellationToken);

            foreach (var entry in timeEntries)
            {
                entry.Approve(approvedBy);
            }
        }

        public async Task<IEnumerable<TimeEntry>> GetDuplicateEntriesAsync(Guid employeeId, Guid projectId, DateTime date, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.EmployeeId == employeeId &&
                           t.ProjectId == projectId &&
                           t.Date.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalHoursByDateRangeAsync(DateTime startDate, DateTime endDate, bool billableOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(t => t.Date >= startDate && t.Date <= endDate);

            if (billableOnly)
                query = query.Where(t => t.Billable);

            return await query.SumAsync(t => t.Hours, cancellationToken);
        }

        public async Task<Dictionary<Guid, decimal>> GetHoursByEmployeeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => t.EmployeeId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Hours), cancellationToken);
        }

        public async Task<Dictionary<Guid, decimal>> GetHoursByProjectAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => t.ProjectId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Hours), cancellationToken);
        }

        public async Task<decimal> GetUtilizationRateAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var totalHours = await GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate, false, cancellationToken);
            var workingDays = CalculateWorkingDays(startDate, endDate);
            var expectedHours = workingDays * 8; // 8 hours per day

            return expectedHours > 0 ? (totalHours / expectedHours) * 100 : 0;
        }

        public async Task<decimal> GetBillableRateAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var totalHours = await GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate, false, cancellationToken);
            var billableHours = await GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate, true, cancellationToken);

            return totalHours > 0 ? (billableHours / totalHours) * 100 : 0;
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
    }
}
