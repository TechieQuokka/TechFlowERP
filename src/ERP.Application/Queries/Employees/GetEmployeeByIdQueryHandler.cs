using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Employees
{
    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<EmployeeDto>("Employee not found");
                }

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Result.Success(employeeDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<EmployeeDto>($"Error retrieving employee: {ex.Message}");
            }
        }
    }
}