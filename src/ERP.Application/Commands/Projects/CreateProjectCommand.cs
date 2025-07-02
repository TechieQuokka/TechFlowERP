using ERP.Application.DTOs;
using ERP.Domain.Enums;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Projects
{
    public class CreateProjectCommand : IRequest<Result<ProjectDto>>
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid ClientId { get; set; }
        public Guid ManagerId { get; set; }
        public ProjectType Type { get; set; } = ProjectType.TimeAndMaterial;
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Medium;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal ProfitMargin { get; set; }
        public List<string> Technologies { get; set; } = new();
        public string CodePrefix { get; set; } = "PRJ";
    }
}