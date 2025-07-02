using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.TimeEntries
{
    public class CreateTimeEntryCommandHandler : IRequestHandler<CreateTimeEntryCommand, Result<TimeEntryDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTimeEntryCommandHandler(
            ITimeEntryRepository timeEntryRepository,
            IEmployeeRepository employeeRepository,
            IProjectRepository projectRepository,
            ICurrentTenantService currentTenantService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _timeEntryRepository = timeEntryRepository;
            _employeeRepository = employeeRepository;
            _projectRepository = projectRepository;
            _currentTenantService = currentTenantService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TimeEntryDto>> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 직원 존재 확인
                var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<TimeEntryDto>("Employee not found");
                }

                // 2. 프로젝트 존재 확인
                var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
                if (project == null)
                {
                    return Result.Failure<TimeEntryDto>("Project not found");
                }

                // 3. 시간 유효성 검증 (Entity에서도 검증하지만 미리 체크)
                if (request.Hours <= 0 || request.Hours > 24)
                {
                    return Result.Failure<TimeEntryDto>("Hours must be between 0.1 and 24");
                }

                // 4. 날짜 유효성 검증 (Entity에서도 검증하지만 미리 체크)
                if (request.Date > DateTime.Today)
                {
                    return Result.Failure<TimeEntryDto>("Cannot log time for future dates");
                }

                // 5. 중복 기록 확인 제거 (데이터베이스 스키마 문제로 인해 임시 제거)
                // TODO: time_entries 테이블에 updated_at 컬럼 추가 후 중복 검사 재활성화

                // 6. 하루 총 시간 제한 확인 제거 (데이터베이스 스키마 문제로 인해 임시 제거)
                // TODO: time_entries 테이블에 updated_at 컬럼 추가 후 시간 제한 검사 재활성화

                // 7. 테넌트 확인
                var tenantId = _currentTenantService.TenantId ?? "default-tenant";

                // 8. TimeEntry 생성 (Domain Entity 생성자 사용)
                var timeEntry = new TimeEntry(
                    tenantId,
                    request.EmployeeId,
                    request.ProjectId,
                    request.Date,
                    request.Hours,
                    request.TaskDescription,
                    request.Billable);

                // 9. 저장
                await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 10. DTO 변환 및 반환
                var timeEntryDto = _mapper.Map<TimeEntryDto>(timeEntry);
                return Result.Success(timeEntryDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<TimeEntryDto>($"Error creating time entry: {ex.Message}");
            }
        }
    }
}