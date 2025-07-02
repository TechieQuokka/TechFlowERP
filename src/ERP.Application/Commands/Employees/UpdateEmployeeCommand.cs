using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class UpdateEmployeeCommand : IRequest<Result<EmployeeDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
    }
}