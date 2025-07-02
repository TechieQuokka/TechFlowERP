using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ErpDbContext context) : base(context) { }

        public override async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Skills)
                .Include(e => e.Assignments)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Skills)
                .FirstOrDefaultAsync(e => e.Email == email, cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Skills)
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Skills)
                .Where(e => e.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetBySkillAsync(string technology, SkillLevel? minLevel = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(e => e.Skills)
                .Where(e => e.Skills.Any(s => s.Technology == technology));

            if (minLevel.HasValue)
            {
                query = query.Where(e => e.Skills.Any(s => s.Technology == technology && s.Level >= minLevel.Value));
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetAvailableEmployeesAsync(DateTime startDate, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            // This is a simplified implementation
            // In practice, you might need to join with ProjectAssignments to check availability
            return await _dbSet
                .Include(e => e.Skills)
                .Include(e => e.Assignments)
                .Where(e => e.Status == EmployeeStatus.Active)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(e => e.Email == email, cancellationToken);
        }
    }
}
