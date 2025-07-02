using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.TimeEntries
{
    public class DeleteTimeEntryCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; }

        public DeleteTimeEntryCommand(Guid id)
        {
            Id = id;
        }
    }
}