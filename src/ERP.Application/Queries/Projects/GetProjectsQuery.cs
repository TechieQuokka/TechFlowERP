using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Projects
{
    public class GetProjectsQuery : IRequest<Result<IEnumerable<ProjectDto>>>
    {
        public Guid? ClientId { get; set; }
        public Guid? ManagerId { get; set; }
        public ProjectStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Technology { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}