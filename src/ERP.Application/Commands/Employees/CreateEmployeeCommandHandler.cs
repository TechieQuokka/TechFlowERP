using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Commands.Employees
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            ICurrentTenantService currentTenantService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _currentTenantService = currentTenantService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 이메일 중복 확인
                if (await _employeeRepository.ExistsByEmailAsync(request.Email, cancellationToken))
                {
                    return Result.Failure<EmployeeDto>("Employee with this email already exists");
                }

                // 2. 직원 생성
                var tenantId = _currentTenantService.TenantId ?? "default-tenant";
                var employee = new Employee(tenantId, request.Name, request.Email, request.HireDate);

                // 3. 추가 정보 설정
                if (!string.IsNullOrEmpty(request.Position) || request.Salary.HasValue)
                {
                    employee.UpdatePosition(request.Position, request.Salary);
                }

                if (request.DepartmentId.HasValue)
                {
                    employee.AssignToDepartment(request.DepartmentId.Value, request.ManagerId);
                }

                if (request.LeaveBalance > 0)
                {
                    employee.UpdateLeaveBalance(request.LeaveBalance);
                }

                // 4. 저장
                await _employeeRepository.AddAsync(employee, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. DTO 변환 및 반환
                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Result.Success(employeeDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<EmployeeDto>($"Error creating employee: {ex.Message}");
            }
        }
    }
}
