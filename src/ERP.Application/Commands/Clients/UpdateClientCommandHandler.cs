using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Clients
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result<ClientDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateClientCommandHandler(
            IClientRepository clientRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 클라이언트 조회
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return Result.Failure<ClientDto>("Client not found");
                }

                // 2. 정보 업데이트
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

                // 3. 저장
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 4. DTO 변환 및 반환
                var clientDto = _mapper.Map<ClientDto>(client);
                return Result.Success(clientDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ClientDto>($"Error updating client: {ex.Message}");
            }
        }
    }
}