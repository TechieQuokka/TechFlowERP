using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Employees
{
    public class GetEmployeesQuery : IRequest<Result<IEnumerable<EmployeeDto>>>
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public EmployeeStatus? Status { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}