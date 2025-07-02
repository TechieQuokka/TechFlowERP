using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Projects
{
    public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Repository의 GetByIdAsync는 이미 Include가 포함되어 있음
                var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

                if (project == null)
                {
                    return Result.Failure<ProjectDto>("Project not found");
                }

                var projectDto = _mapper.Map<ProjectDto>(project);
                return Result.Success(projectDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectDto>($"Error retrieving project: {ex.Message}");
            }
        }
    }
}