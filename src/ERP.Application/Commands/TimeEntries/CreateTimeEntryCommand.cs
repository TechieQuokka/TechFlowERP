using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.TimeEntries
{
    public class CreateTimeEntryCommand : IRequest<Result<TimeEntryDto>>
    {
        public Guid EmployeeId { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string? TaskDescription { get; set; }
        public bool Billable { get; set; } = true;
    }
}