using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Clients
{
    public class GetClientProjectsQueryHandler : IRequestHandler<GetClientProjectsQuery, Result<IEnumerable<ProjectDto>>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetClientProjectsQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProjectDto>>> Handle(GetClientProjectsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var projects = await _projectRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
                var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
                return Result.Success(projectDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<ProjectDto>>($"Error retrieving client projects: {ex.Message}");
            }
        }
    }
}