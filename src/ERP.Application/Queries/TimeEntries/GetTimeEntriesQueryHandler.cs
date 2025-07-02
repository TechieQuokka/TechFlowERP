using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.TimeEntries
{
    public class GetTimeEntriesQueryHandler : IRequestHandler<GetTimeEntriesQuery, Result<IEnumerable<TimeEntryDto>>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IMapper _mapper;

        public GetTimeEntriesQueryHandler(
            ITimeEntryRepository timeEntryRepository,
            IMapper mapper)
        {
            _timeEntryRepository = timeEntryRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<TimeEntryDto>>> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Domain.Entities.TimeEntry> timeEntries;

                // 필터링 조건에 따라 다른 Repository 메서드 사용
                if (request.EmployeeId.HasValue)
                {
                    // 직원별 조회 (updated_at 컬럼 문제가 없는 메서드 사용 필요)
                    // 임시로 간단한 조회만 구현
                    var allEntries = await _timeEntryRepository.GetAllAsync(cancellationToken);
                    timeEntries = allEntries.Where(te => te.EmployeeId == request.EmployeeId.Value);
                }
                else if (request.ProjectId.HasValue)
                {
                    // 프로젝트별 조회 (updated_at 컬럼 문제가 없는 메서드 사용 필요)
                    var allEntries = await _timeEntryRepository.GetAllAsync(cancellationToken);
                    timeEntries = allEntries.Where(te => te.ProjectId == request.ProjectId.Value);
                }
                else
                {
                    // 전체 조회
                    timeEntries = await _timeEntryRepository.GetAllAsync(cancellationToken);
                }

                // 메모리에서 추가 필터링
                var filteredEntries = timeEntries.AsQueryable();

                if (request.StartDate.HasValue)
                {
                    filteredEntries = filteredEntries.Where(te => te.Date >= request.StartDate.Value.Date);
                }

                if (request.EndDate.HasValue)
                {
                    filteredEntries = filteredEntries.Where(te => te.Date <= request.EndDate.Value.Date);
                }

                if (request.Billable.HasValue)
                {
                    filteredEntries = filteredEntries.Where(te => te.Billable == request.Billable.Value);
                }

                if (request.Approved.HasValue)
                {
                    filteredEntries = filteredEntries.Where(te => te.Approved == request.Approved.Value);
                }

                // 정렬 및 페이징
                var pagedEntries = filteredEntries
                    .OrderByDescending(te => te.Date)
                    .ThenByDescending(te => te.CreatedAt)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToList();

                // DTO 변환
                var timeEntryDtos = _mapper.Map<IEnumerable<TimeEntryDto>>(pagedEntries);
                return Result.Success(timeEntryDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<TimeEntryDto>>($"Error retrieving time entries: {ex.Message}");
            }
        }
    }
}