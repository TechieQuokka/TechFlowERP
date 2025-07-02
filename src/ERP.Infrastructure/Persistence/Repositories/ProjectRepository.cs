using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using ERP.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ErpDbContext context) : base(context) { }

        // GetAllAsync 오버라이드 - Include 추가
        public override async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Include(p => p.Assignments)
                .Include(p => p.Milestones)
                .ToListAsync(cancellationToken);
        }

        public override async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Include(p => p.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Project?> GetByCodeAsync(ProjectCode code, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .FirstOrDefaultAsync(p => p.Code == code.Value, cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Where(p => p.ClientId == clientId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Where(p => p.ManagerId == managerId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Where(p => p.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Where(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Planning)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetProjectsInDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Client)
                .Include(p => p.Manager)
                .Where(p => p.Period.StartDate <= endDate &&
                           (p.Period.EndDate == null || p.Period.EndDate >= startDate))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByCodeAsync(ProjectCode code, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(p => p.Code == code.Value, cancellationToken);
        }

        public async Task<int> GetProjectCountByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(p => p.ClientId == clientId, cancellationToken);
        }
    }
}