using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Clients
{
    public class GetClientsQuery : IRequest<Result<IEnumerable<ClientDto>>>
    {
        public string? Industry { get; set; }
        public string? ClientSize { get; set; }
        public string? CompanyName { get; set; }
        public decimal? MinContractValue { get; set; }
        public decimal? MaxContractValue { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}