using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Clients
{
    public class GetClientByIdQuery : IRequest<Result<ClientDto>>
    {
        public Guid Id { get; set; }

        public GetClientByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}