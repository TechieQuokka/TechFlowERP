using ERP.Domain.Interfaces;
using ERP.Domain.Enums;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;
using MediatR;

namespace ERP.Application.Commands.Projects
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result<bool>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProjectCommandHandler(
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 프로젝트 조회
                var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
                if (project == null)
                {
                    return Result.Failure<bool>("Project not found");
                }

                // 2. 활성 상태 프로젝트인지 확인
                if (project.Status == ProjectStatus.Active)
                {
                    return Result.Failure<bool>("Cannot delete active project. Please change status to Planning, OnHold, Completed, or Cancelled first.");
                }

                // 3. 진행률이 있는지 확인 (마일스톤이 완료된 경우)
                var progress = project.CalculateProgress();
                if (progress > 0)
                {
                    return Result.Failure<bool>("Cannot delete project with completed milestones. Progress: " + progress.ToString("0.##") + "%");
                }

                // 4. 프로젝트에 할당된 직원이 있는지 확인
                if (project.Assignments.Any())
                {
                    return Result.Failure<bool>("Cannot delete project with employee assignments. Please remove all assignments first.");
                }

                // 5. 예산이 설정되어 있고 실제 비용이 발생한 경우 확인
                var estimatedCost = project.CalculateEstimatedCost();
                if (estimatedCost.Amount > 0)
                {
                    return Result.Failure<bool>("Cannot delete project with estimated costs. Project has financial commitments.");
                }

                // 6. 마일스톤이 있는 경우 확인
                if (project.Milestones.Any())
                {
                    return Result.Failure<bool>("Cannot delete project with milestones. Please remove all milestones first.");
                }

                // 7. 삭제 실행
                await _projectRepository.DeleteAsync(project, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error deleting project: {ex.Message}");
            }
        }
    }
}