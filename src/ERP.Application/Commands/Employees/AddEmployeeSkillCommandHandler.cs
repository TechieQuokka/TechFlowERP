using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class AddEmployeeSkillCommandHandler : IRequestHandler<AddEmployeeSkillCommand, Result<bool>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddEmployeeSkillCommandHandler(
            IEmployeeRepository employeeRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(AddEmployeeSkillCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<bool>("Employee not found");
                }

                employee.AddOrUpdateSkill(
                    request.Technology,
                    request.Level,
                    request.YearsExperience,
                    request.Certification);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error adding employee skill: {ex.Message}");
            }
        }
    }
}