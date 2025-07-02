using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class CreateEmployeeCommand : IRequest<Result<EmployeeDto>>
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public int LeaveBalance { get; set; } = 20; // Default 20 days
    }
}
