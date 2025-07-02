using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<EmployeeDto>("Employee not found");
                }

                // 기본 정보 업데이트
                employee.UpdatePersonalInfo(request.Name, request.Email);

                // 직책 및 급여 업데이트
                employee.UpdatePosition(request.Position, request.Salary);

                // 부서 및 매니저 할당
                if (request.DepartmentId.HasValue)
                {
                    employee.AssignToDepartment(request.DepartmentId.Value, request.ManagerId);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Result.Success(employeeDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<EmployeeDto>($"Error updating employee: {ex.Message}");
            }
        }
    }
}