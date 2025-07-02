using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.SharedKernel.Abstractions;

namespace ERP.Domain.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetBySkillAsync(string technology, SkillLevel? minLevel = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetAvailableEmployeesAsync(DateTime startDate, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}