using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ErpDbContext context) : base(context) { }

        public override async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.LineItems)
                .Include(i => i.Project)
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.LineItems)
                .Include(i => i.Project)
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
        }

        public async Task<IEnumerable<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.LineItems)
                .Include(i => i.Project)
                .Where(i => i.ClientId == clientId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Invoice>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.LineItems)
                .Include(i => i.Client)
                .Where(i => i.ProjectId == projectId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.LineItems)
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Where(i => i.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.LineItems)
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.Today)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
        }

        public async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
        {
            var currentYear = DateTime.Now.Year;
            var prefix = $"INV-{currentYear}-";

            var lastInvoice = await _dbSet
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastInvoice == null)
            {
                return $"{prefix}0001";
            }

            var lastNumberPart = lastInvoice.InvoiceNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberPart, out var lastNumber))
            {
                return $"{prefix}{(lastNumber + 1):D4}";
            }

            return $"{prefix}0001";
        }
    }
}