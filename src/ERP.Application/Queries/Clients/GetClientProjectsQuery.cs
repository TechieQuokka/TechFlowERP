using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Clients
{
    public class GetClientProjectsQuery : IRequest<Result<IEnumerable<ProjectDto>>>
    {
        public Guid ClientId { get; }

        public GetClientProjectsQuery(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}