using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.Domain.Services;
using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Projects
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ProjectDomainService _projectDomainService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateProjectCommandHandler(
            IProjectRepository projectRepository,
            IClientRepository clientRepository,
            IEmployeeRepository employeeRepository,
            ProjectDomainService projectDomainService,
            ICurrentTenantService currentTenantService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _clientRepository = clientRepository;
            _employeeRepository = employeeRepository;
            _projectDomainService = projectDomainService;
            _currentTenantService = currentTenantService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 클라이언트 존재 확인
                var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
                if (client == null)
                {
                    return Result.Failure<ProjectDto>("Client not found");
                }

                // 2. 매니저 존재 확인
                var manager = await _employeeRepository.GetByIdAsync(request.ManagerId, cancellationToken);
                if (manager == null)
                {
                    return Result.Failure<ProjectDto>("Manager not found");
                }

                // 3. 고유한 프로젝트 코드 생성
                var codeResult = await _projectDomainService.GenerateUniqueProjectCodeAsync(
                    request.CodePrefix, request.StartDate, cancellationToken);

                if (codeResult.IsFailure)
                {
                    return Result.Failure<ProjectDto>(codeResult.Errors);
                }

                // 4. 프로젝트 생성
                var tenantId = _currentTenantService.TenantId ?? "default-tenant";
                var period = new DateRange(request.StartDate, request.EndDate);
                var budget = new Money(request.Budget, request.Currency);

                var project = new Project(
                    tenantId,
                    codeResult.Value,
                    request.Name,
                    request.ClientId,
                    request.ManagerId,
                    period,
                    budget);

                // 5. 추가 정보 설정
                project.UpdateBasicInfo(request.Name, request.Description);
                project.UpdateBudget(budget, request.ProfitMargin);

                // 6. 기술 스택 추가
                foreach (var technology in request.Technologies)
                {
                    project.AddTechnology(technology);
                }

                // 7. 저장
                await _projectRepository.AddAsync(project, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 8. DTO 변환 및 반환
                var projectDto = _mapper.Map<ProjectDto>(project);
                return Result.Success(projectDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProjectDto>($"Error creating project: {ex.Message}");
            }
        }
    }
}