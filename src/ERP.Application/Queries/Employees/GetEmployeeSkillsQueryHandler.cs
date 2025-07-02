using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Interfaces;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Employees
{
    public class GetEmployeeSkillsQueryHandler : IRequestHandler<GetEmployeeSkillsQuery, Result<IEnumerable<EmployeeSkillDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public GetEmployeeSkillsQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<EmployeeSkillDto>>> Handle(GetEmployeeSkillsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
                if (employee == null)
                {
                    return Result.Failure<IEnumerable<EmployeeSkillDto>>("Employee not found");
                }

                var skillDtos = _mapper.Map<IEnumerable<EmployeeSkillDto>>(employee.Skills);
                return Result.Success(skillDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<EmployeeSkillDto>>($"Error retrieving employee skills: {ex.Message}");
            }
        }
    }
}