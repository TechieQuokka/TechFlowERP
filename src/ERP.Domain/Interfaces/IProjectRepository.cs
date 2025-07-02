using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Abstractions;

namespace ERP.Domain.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<Project?> GetByCodeAsync(ProjectCode code, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectsInDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<bool> ExistsByCodeAsync(ProjectCode code, CancellationToken cancellationToken = default);
        Task<int> GetProjectCountByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
    }
}
