using ERP.Domain.Enums;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class AddEmployeeSkillCommand : IRequest<Result<bool>>
    {
        public Guid EmployeeId { get; set; }
        public string Technology { get; set; } = default!;
        public SkillLevel Level { get; set; }
        public int YearsExperience { get; set; }
        public string? Certification { get; set; }
    }
}