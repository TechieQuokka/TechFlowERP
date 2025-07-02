using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Clients
{
    public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, Result<IEnumerable<ClientDto>>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public GetClientsQueryHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ClientDto>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 모든 클라이언트 조회
                var clients = await _clientRepository.GetAllAsync(cancellationToken);

                // 2. 필터링
                var filteredClients = clients.AsEnumerable();

                if (!string.IsNullOrEmpty(request.Industry))
                {
                    filteredClients = filteredClients.Where(c =>
                        c.Industry != null && c.Industry.Contains(request.Industry, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(request.ClientSize))
                {
                    filteredClients = filteredClients.Where(c =>
                        c.ClientSize != null && c.ClientSize.Equals(request.ClientSize, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(request.CompanyName))
                {
                    filteredClients = filteredClients.Where(c =>
                        c.CompanyName.Contains(request.CompanyName, StringComparison.OrdinalIgnoreCase));
                }

                if (request.MinContractValue.HasValue)
                {
                    filteredClients = filteredClients.Where(c =>
                        c.ContractValue.HasValue && c.ContractValue >= request.MinContractValue);
                }

                if (request.MaxContractValue.HasValue)
                {
                    filteredClients = filteredClients.Where(c =>
                        c.ContractValue.HasValue && c.ContractValue <= request.MaxContractValue);
                }

                // 3. 페이징
                var pagedClients = filteredClients
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToList();

                // 4. DTO 변환
                var clientDtos = _mapper.Map<IEnumerable<ClientDto>>(pagedClients);
                return Result.Success(clientDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<ClientDto>>($"Error retrieving clients: {ex.Message}");
            }
        }
    }
}