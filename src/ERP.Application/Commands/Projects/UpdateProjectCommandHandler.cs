using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Projects
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateProjectCommandHandler(
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 프로젝트 조회
                var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
                if (project == null)
                {
                    return Result.Failure<ProjectDto>("Project not found");
                }

                // 2. 기본 정보 업데이트 (실제 Entity 메서드)
                project.UpdateBasicInfo(request.Name, request.Description);

                // 3. 예산 및 수익률 업데이트 (실제 Entity 메서드)
                var currentCurrency = project.Budget?.Currency ?? "USD";
                var newBudget = new Money(request.Budget, currentCurrency);
                project.UpdateBudget(newBudget, request.ProfitMargin);

                // 4. 위험도 업데이트 (Property setter - private이므로 reflection 또는 새 생성자 필요)
                // RiskLevel은 private setter이므로 직접 업데이트 불가능
                // 필요시 Entity에 UpdateRiskLevel 메서드 추가 필요

                // 5. 종료일 업데이트 (Period는 private setter이므로 직접 업데이트 불가능)
                // Period도 private setter이므로 직접 업데이트 불가능
                // 필요시 Entity에 UpdatePeriod 메서드 추가 필요

                // 6. 기술 스택 업데이트 (실제 Entity 메서드들)
                // 기존 기술들을 먼저 제거
                var currentTechnologies = project.Technologies.ToList();
                foreach (var tech in currentTechnologies)
                {
                    project.RemoveTechnology(tech);
                }

                // 새로운 기술들 추가
                foreach (var technology in request.Technologies)
                {
                    if (!string.IsNullOrWhiteSpace(technology))
                    {
                        project.AddTechnology(technology);
                    }
                }

                // 7. Repository를 통해 업데이트
                await _projectRepository.UpdateAsync(project, cancellationToken);

                // 8. UnitOfWork로 변경사항 저장
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 9. DTO 변환 및 반환
                var projectDto = _mapper.Map<ProjectDto>(project);
                return Result.Success(projectDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectDto>($"Error updating project: {ex.Message}");
            }
        }
    }
}