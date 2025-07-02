using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Employees
{
    public class GetEmployeeByIdQuery : IRequest<Result<EmployeeDto>>
    {
        public Guid Id { get; }

        public GetEmployeeByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}