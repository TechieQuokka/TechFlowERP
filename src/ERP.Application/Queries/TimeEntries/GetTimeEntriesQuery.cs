using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.TimeEntries
{
    public class GetTimeEntriesQuery : IRequest<Result<IEnumerable<TimeEntryDto>>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? Billable { get; set; }
        public bool? Approved { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}