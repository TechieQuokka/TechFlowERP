using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Projects
{
    public class DeleteProjectCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; }

        public DeleteProjectCommand(Guid id)
        {
            Id = id;
        }
    }
}