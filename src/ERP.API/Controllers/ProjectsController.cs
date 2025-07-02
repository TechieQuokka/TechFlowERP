using ERP.Application.Commands.Projects;
using ERP.Application.DTOs;
using ERP.Application.Queries.Projects;
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
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ProjectApplicationService _projectService;

        public ProjectsController(IMediator mediator, ProjectApplicationService projectService)
        {
            _mediator = mediator;
            _projectService = projectService;
        }

        /// <summary>
        /// Get all projects with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects(
            [FromQuery] Guid? clientId,
            [FromQuery] Guid? managerId,
            [FromQuery] ProjectStatus? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? technology,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var query = new GetProjectsQuery
            {
                ClientId = clientId,
                ManagerId = managerId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                Technology = technology,
                Skip = skip,
                Take = take
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
        {
            var query = new GetProjectByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetProject), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<bool>> DeleteProject(Guid id)
        {
            var command = new DeleteProjectCommand(id);
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get project profitability analysis
        /// </summary>
        [HttpGet("{id:guid}/profitability")]
        public async Task<ActionResult<ProjectProfitabilityDto>> GetProjectProfitability(Guid id)
        {
            var result = await _projectService.GetProjectProfitabilityAsync(id);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get project resource utilization
        /// </summary>
        [HttpGet("{id:guid}/utilization")]
        public async Task<ActionResult<IEnumerable<ProjectResourceUtilizationDto>>> GetResourceUtilization(
            Guid id,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var result = await _projectService.GetResourceUtilizationAsync(id, startDate, endDate);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }
    }
}