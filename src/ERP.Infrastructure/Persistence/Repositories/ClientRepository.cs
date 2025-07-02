using ERP.Domain.Entities;
using ERP.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(ErpDbContext context) : base(context) { }

        public async Task<IEnumerable<Client>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Industry == industry)
                .ToListAsync(cancellationToken);
        }

        public async Task<Client?> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.CompanyName == companyName, cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetClientsWithActiveProjectsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Projects)
                .Where(c => c.Projects.Any(p => p.Status == Domain.Enums.ProjectStatus.Active))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(c => c.CompanyName == companyName, cancellationToken);
        }
    }
}