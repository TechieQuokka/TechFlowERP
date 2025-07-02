using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.TimeEntries
{
    public class UpdateTimeEntryCommandHandler : IRequestHandler<UpdateTimeEntryCommand, Result<TimeEntryDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateTimeEntryCommandHandler(
            ITimeEntryRepository timeEntryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _timeEntryRepository = timeEntryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TimeEntryDto>> Handle(UpdateTimeEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 시간 기록 조회
                var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);
                if (timeEntry == null)
                {
                    return Result.Failure<TimeEntryDto>("Time entry not found");
                }

                // 2. 승인된 기록은 수정 불가 (Entity에서 자동으로 검증됨)
                if (timeEntry.Approved)
                {
                    return Result.Failure<TimeEntryDto>("Cannot update approved time entry");
                }

                // 3. 날짜 유효성 검증 (Date는 Entity에서 private setter이므로 변경 불가)
                if (request.Date.Date != timeEntry.Date.Date)
                {
                    return Result.Failure<TimeEntryDto>("Cannot change the date of existing time entry. Please create a new entry for different date.");
                }

                // 4. 하루 총 시간 제한 확인 (현재 기록 제외)
                var dailyTotal = await _timeEntryRepository.GetTotalHoursByEmployeeAsync(
                    timeEntry.EmployeeId, timeEntry.Date, timeEntry.Date.AddDays(1), false, cancellationToken);

                var newDailyTotal = dailyTotal - timeEntry.Hours + request.Hours;

                if (newDailyTotal > 24)
                {
                    return Result.Failure<TimeEntryDto>($"Total daily hours cannot exceed 24. Current total: {dailyTotal}, New total would be: {newDailyTotal}");
                }

                // 5. TimeEntry 업데이트 (실제 Entity 메서드들 사용)
                timeEntry.UpdateHours(request.Hours);
                timeEntry.UpdateDescription(request.TaskDescription);
                timeEntry.SetBillable(request.Billable);

                // 6. 저장
                await _timeEntryRepository.UpdateAsync(timeEntry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 7. DTO 변환 및 반환
                var timeEntryDto = _mapper.Map<TimeEntryDto>(timeEntry);
                return Result.Success(timeEntryDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<TimeEntryDto>($"Error updating time entry: {ex.Message}");
            }
        }
    }
}