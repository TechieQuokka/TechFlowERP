using ERP.Application.Commands.Clients;
using ERP.Application.DTOs;
using ERP.Application.Queries.Clients;

using ERP.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all clients with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients(
            [FromQuery] string? industry,
            [FromQuery] string? clientSize,
            [FromQuery] string? companyName,
            [FromQuery] decimal? minContractValue,
            [FromQuery] decimal? maxContractValue,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var query = new GetClientsQuery
            {
                Industry = industry,
                ClientSize = clientSize,
                CompanyName = companyName,
                MinContractValue = minContractValue,
                MaxContractValue = maxContractValue,
                Skip = skip,
                Take = take
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get client by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientDto>> GetClient(Guid id)
        {
            var query = new GetClientByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetClient), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Update an existing client
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, [FromBody] UpdateClientCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Delete a client
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var command = new DeleteClientCommand(id);
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return NoContent();
        }

        /// <summary>
        /// Get client projects
        /// </summary>
        [HttpGet("{id:guid}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetClientProjects(Guid id)
        {
            var query = new GetClientProjectsQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }


    }
}