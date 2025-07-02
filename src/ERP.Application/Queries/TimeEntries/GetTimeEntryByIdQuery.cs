using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.TimeEntries
{
    public class GetTimeEntryByIdQuery : IRequest<Result<TimeEntryDto>>
    {
        public Guid Id { get; }

        public GetTimeEntryByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}