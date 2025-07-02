using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Clients
{
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public GetClientByIdQueryHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<Result<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return Result.Failure<ClientDto>("Client not found");
                }

                var clientDto = _mapper.Map<ClientDto>(client);
                return Result.Success(clientDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ClientDto>($"Error retrieving client: {ex.Message}");
            }
        }
    }
}