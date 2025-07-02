using ERP.Application.Commands.Employees;
using ERP.Application.DTOs;
using ERP.Application.Queries.Employees;
using ERP.Application.Services;
using ERP.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly EmployeeApplicationService _employeeService;

        public EmployeesController(IMediator mediator, EmployeeApplicationService employeeService)
        {
            _mediator = mediator;
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all employees with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
            [FromQuery] string? name,
            [FromQuery] string? email,
            [FromQuery] EmployeeStatus? status,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? managerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var query = new GetEmployeesQuery
            {
                Name = name,
                Email = email,
                Status = status,
                DepartmentId = departmentId,
                ManagerId = managerId,
                Skip = skip,
                Take = take
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid id)
        {
            var query = new GetEmployeeByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Create a new employee
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetEmployee), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Update an existing employee
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Delete an employee
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var command = new DeleteEmployeeCommand(id);
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return NoContent();
        }

        /// <summary>
        /// Get available employees for project assignment
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAvailableEmployees(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? requiredSkill,
            [FromQuery] SkillLevel? minSkillLevel)
        {
            var result = await _employeeService.GetAvailableEmployeesAsync(
                startDate, endDate, requiredSkill, minSkillLevel);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get employee performance metrics
        /// </summary>
        [HttpGet("{id:guid}/performance")]
        public async Task<ActionResult<EmployeePerformanceDto>> GetEmployeePerformance(
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _employeeService.GetEmployeePerformanceAsync(id, startDate, endDate);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get employee skills
        /// </summary>
        [HttpGet("{id:guid}/skills")]
        public async Task<ActionResult<IEnumerable<EmployeeSkillDto>>> GetEmployeeSkills(Guid id)
        {
            var query = new GetEmployeeSkillsQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Add skill to employee
        /// </summary>
        [HttpPost("{id:guid}/skills")]
        public async Task<ActionResult> AddEmployeeSkill(Guid id, [FromBody] AddEmployeeSkillCommand command)
        {
            command.EmployeeId = id;
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok();
        }
    }
}