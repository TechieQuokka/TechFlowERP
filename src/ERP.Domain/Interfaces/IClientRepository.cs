using ERP.Domain.Entities;
using ERP.SharedKernel.Abstractions;

namespace ERP.Domain.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<IEnumerable<Client>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);
        Task<Client?> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetClientsWithActiveProjectsAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);
    }
}