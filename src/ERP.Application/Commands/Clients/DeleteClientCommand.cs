using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Clients
{
    public class DeleteClientCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; }

        public DeleteClientCommand(Guid id)
        {
            Id = id;
        }
    }
}