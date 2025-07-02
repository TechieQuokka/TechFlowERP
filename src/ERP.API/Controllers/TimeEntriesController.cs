using ERP.Application.Commands.TimeEntries;
using ERP.Application.DTOs;
using ERP.Application.Queries.TimeEntries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class TimeEntriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimeEntriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new time entry
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TimeEntryDto>> CreateTimeEntry([FromBody] CreateTimeEntryCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetTimeEntry), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Get time entry by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TimeEntryDto>> GetTimeEntry(Guid id)
        {
            var query = new GetTimeEntryByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get time entries by employee
        /// </summary>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<ActionResult<IEnumerable<TimeEntryDto>>> GetTimeEntriesByEmployee(
            Guid employeeId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var query = new GetTimeEntriesQuery
            {
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
                Take = 100 // 더 많은 데이터 조회
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get all time entries with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeEntryDto>>> GetTimeEntries(
            [FromQuery] Guid? employeeId,
            [FromQuery] Guid? projectId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? billable,
            [FromQuery] bool? approved,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var query = new GetTimeEntriesQuery
            {
                EmployeeId = employeeId,
                ProjectId = projectId,
                StartDate = startDate,
                EndDate = endDate,
                Billable = billable,
                Approved = approved,
                Skip = skip,
                Take = take
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Get pending approval time entries
        /// </summary>
        [HttpGet("pending-approval")]
        public async Task<ActionResult<IEnumerable<TimeEntryDto>>> GetPendingApproval(
            [FromQuery] Guid? managerId)
        {
            var query = new GetTimeEntriesQuery
            {
                Approved = false, // 승인되지 않은 항목만
                Take = 100
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Update an existing time entry
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TimeEntryDto>> UpdateTimeEntry(Guid id, [FromBody] UpdateTimeEntryCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Delete a time entry
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<bool>> DeleteTimeEntry(Guid id)
        {
            var command = new DeleteTimeEntryCommand(id);
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        /// <summary>
        /// Approve time entry (간단한 버전)
        /// </summary>
        [HttpPatch("{id:guid}/approve")]
        public async Task<ActionResult> ApproveTimeEntry(Guid id)
        {
            // TODO: ApproveTimeEntryCommand 구현 필요
            // 현재는 간단한 응답만 반환
            return Ok(new { message = "Approve functionality not yet implemented", timeEntryId = id });
        }
    }
}