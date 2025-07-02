using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Clients
{
    public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Result<bool>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClientCommandHandler(
            IClientRepository clientRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 클라이언트 조회
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return Result.Failure<bool>("Client not found");
                }

                // 2. 연관된 프로젝트 확인
                var projects = await _projectRepository.GetByClientIdAsync(request.Id, cancellationToken);
                if (projects.Any())
                {
                    return Result.Failure<bool>("Cannot delete client with existing projects");
                }

                // 3. 삭제
                await _clientRepository.DeleteAsync(client, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error deleting client: {ex.Message}");
            }
        }
    }
}