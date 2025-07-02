using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Clients
{
    public class CreateClientCommand : IRequest<Result<ClientDto>>
    {
        public string CompanyName { get; set; } = default!;
        public string? Industry { get; set; }
        public string? ClientSize { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public decimal? ContractValue { get; set; }
    }
}