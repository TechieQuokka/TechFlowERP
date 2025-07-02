using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Employees
{
    public class GetEmployeeSkillsQuery : IRequest<Result<IEnumerable<EmployeeSkillDto>>>
    {
        public Guid EmployeeId { get; }

        public GetEmployeeSkillsQuery(Guid employeeId)
        {
            EmployeeId = employeeId;
        }
    }
}