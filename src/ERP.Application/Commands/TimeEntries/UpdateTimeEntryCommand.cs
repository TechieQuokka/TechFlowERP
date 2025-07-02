using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.TimeEntries
{
    public class UpdateTimeEntryCommand : IRequest<Result<TimeEntryDto>>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string? TaskDescription { get; set; }
        public bool Billable { get; set; }
    }
}