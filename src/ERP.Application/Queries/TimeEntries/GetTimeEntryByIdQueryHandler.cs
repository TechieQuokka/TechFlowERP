using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.TimeEntries
{
    public class GetTimeEntryByIdQueryHandler : IRequestHandler<GetTimeEntryByIdQuery, Result<TimeEntryDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IMapper _mapper;

        public GetTimeEntryByIdQueryHandler(
            ITimeEntryRepository timeEntryRepository,
            IMapper mapper)
        {
            _timeEntryRepository = timeEntryRepository;
            _mapper = mapper;
        }

        public async Task<Result<TimeEntryDto>> Handle(GetTimeEntryByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);
                if (timeEntry == null)
                {
                    return Result.Failure<TimeEntryDto>("Time entry not found");
                }

                var timeEntryDto = _mapper.Map<TimeEntryDto>(timeEntry);
                return Result.Success(timeEntryDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<TimeEntryDto>($"Error retrieving time entry: {ex.Message}");
            }
        }
    }
}