using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<bool>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<bool>("Employee not found");
                }

                // 활성 프로젝트 할당 확인
                var activeAssignments = employee.GetActiveAssignments();
                if (activeAssignments.Any())
                {
                    return Result.Failure<bool>("Cannot delete employee with active project assignments");
                }

                // 매니저로 있는 프로젝트 확인
                var managedProjects = await _projectRepository.GetByManagerIdAsync(request.Id, cancellationToken);
                if (managedProjects.Any())
                {
                    return Result.Failure<bool>("Cannot delete employee who is managing active projects");
                }

                await _employeeRepository.DeleteAsync(employee, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error deleting employee: {ex.Message}");
            }
        }
    }
}