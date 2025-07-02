using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Employees
{
    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<IEnumerable<EmployeeDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public GetEmployeesQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 모든 직원 조회
                var employees = await _employeeRepository.GetAllAsync(cancellationToken);

                // 필터링
                var filteredEmployees = employees.AsEnumerable();

                if (!string.IsNullOrEmpty(request.Name))
                {
                    filteredEmployees = filteredEmployees.Where(e =>
                        e.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    filteredEmployees = filteredEmployees.Where(e =>
                        e.Email.Contains(request.Email, StringComparison.OrdinalIgnoreCase));
                }

                if (request.Status.HasValue)
                {
                    filteredEmployees = filteredEmployees.Where(e => e.Status == request.Status.Value);
                }

                if (request.DepartmentId.HasValue)
                {
                    filteredEmployees = filteredEmployees.Where(e => e.DepartmentId == request.DepartmentId.Value);
                }

                if (request.ManagerId.HasValue)
                {
                    filteredEmployees = filteredEmployees.Where(e => e.ManagerId == request.ManagerId.Value);
                }

                // 페이징
                var pagedEmployees = filteredEmployees
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToList();

                // DTO 변환
                var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(pagedEmployees);
                return Result.Success(employeeDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<EmployeeDto>>($"Error retrieving employees: {ex.Message}");
            }
        }
    }
}