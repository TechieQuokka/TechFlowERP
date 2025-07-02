using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.TimeEntries
{
    public class DeleteTimeEntryCommandHandler : IRequestHandler<DeleteTimeEntryCommand, Result<bool>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTimeEntryCommandHandler(
            ITimeEntryRepository timeEntryRepository,
            IUnitOfWork unitOfWork)
        {
            _timeEntryRepository = timeEntryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 시간 기록 조회
                var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);
                if (timeEntry == null)
                {
                    return Result.Failure<bool>("Time entry not found");
                }

                // 2. 승인된 기록은 삭제 불가
                if (timeEntry.Approved)
                {
                    return Result.Failure<bool>("Cannot delete approved time entry");
                }

                // 3. 삭제 실행
                await _timeEntryRepository.DeleteAsync(timeEntry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error deleting time entry: {ex.Message}");
            }
        }
    }
}