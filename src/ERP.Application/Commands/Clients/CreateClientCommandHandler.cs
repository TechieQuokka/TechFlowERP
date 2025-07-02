using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Clients
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateClientCommandHandler(
            IClientRepository clientRepository,
            ICurrentTenantService currentTenantService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _clientRepository = clientRepository;
            _currentTenantService = currentTenantService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 테넌트 확인
                var tenantId = _currentTenantService.TenantId ?? "default-tenant";

                // 2. 중복 회사명 확인
                var existingClient = await _clientRepository.GetByCompanyNameAsync(request.CompanyName, cancellationToken);
                if (existingClient != null)
                {
                    return Result.Failure<ClientDto>("Client with this company name already exists");
                }

                // 3. 클라이언트 생성
                var client = new Client(
                    tenantId,
                    request.CompanyName,
                    request.Industry);

                // 4. 추가 정보 설정
                client.UpdateBasicInfo(
                    request.CompanyName,
                    request.Industry,
                    request.ClientSize);

                client.UpdateContactInfo(
                    request.ContactPerson,
                    request.ContactEmail,
                    request.ContactPhone);

                if (!string.IsNullOrEmpty(request.Address))
                {
                    client.UpdateAddress(request.Address);
                }

                if (request.ContractValue.HasValue)
                {
                    client.SetContractValue(request.ContractValue.Value);
                }

                // 5. 저장
                await _clientRepository.AddAsync(client, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 6. DTO 변환 및 반환
                var clientDto = _mapper.Map<ClientDto>(client);
                return Result.Success(clientDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ClientDto>($"Error creating client: {ex.Message}");
            }
        }
    }
}