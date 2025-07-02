using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Projects
{
    public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<IEnumerable<ProjectDto>>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectsQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Domain.Entities.Project> projects;

                // 조건별 조회
                if (request.ClientId.HasValue)
                {
                    projects = await _projectRepository.GetByClientIdAsync(request.ClientId.Value, cancellationToken);
                }
                else if (request.ManagerId.HasValue)
                {
                    projects = await _projectRepository.GetByManagerIdAsync(request.ManagerId.Value, cancellationToken);
                }
                else if (request.Status.HasValue)
                {
                    projects = await _projectRepository.GetByStatusAsync(request.Status.Value, cancellationToken);
                }
                else
                {
                    projects = await _projectRepository.GetAllAsync(cancellationToken);
                }

                // 추가 필터링
                var filteredProjects = projects.AsEnumerable();

                if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    filteredProjects = filteredProjects.Where(p =>
                        p.Period.StartDate >= request.StartDate &&
                        (p.Period.EndDate == null || p.Period.EndDate <= request.EndDate));
                }

                if (!string.IsNullOrEmpty(request.Technology))
                {
                    filteredProjects = filteredProjects.Where(p =>
                        p.Technologies.Any(t => t.Contains(request.Technology, StringComparison.OrdinalIgnoreCase)));
                }

                // 페이징
                var pagedProjects = filteredProjects
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToList();

                // DTO 변환
                var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(pagedProjects);

                return Result.Success(projectDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<ProjectDto>>($"Error retrieving projects: {ex.Message}");
            }
        }
    }
}