using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class DeleteEmployeeCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; }

        public DeleteEmployeeCommand(Guid id)
        {
            Id = id;
        }
    }
}