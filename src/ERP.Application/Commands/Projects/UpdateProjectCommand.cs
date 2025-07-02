using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Projects
{
    public class UpdateProjectCommand : IRequest<Result<ProjectDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Budget { get; set; }
        public decimal ProfitMargin { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Technologies { get; set; } = new();
    }
}