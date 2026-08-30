using LegalDocsPro.Application.Features.Contracts.Commands;
using LegalDocsPro.Application.Features.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContractsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error, code = result.ErrorCode });

            return CreatedAtAction(nameof(CreateContract), new { id = result.Value }, result.Value);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById(int id)
        {
            var query = new GetContractByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { error = $"Contract with ID {id} not found." });

            return Ok(result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] GetContractsWithPaginationQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPatch("{id}/send-to-review")]
        public async Task<IActionResult> SendToReview(int id)
        {
            var result = await _mediator.Send(new SendContractToReviewCommand(id));

            if (result.IsFailure)
                return BadRequest(new { error = result.Error, code = result.ErrorCode });

            return NoContent();
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _mediator.Send(new ContractApproveCommand(id));

            if (result.IsFailure)
                return BadRequest(new { error = result.Error, code = result.ErrorCode });

            return NoContent();
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _mediator.Send(new ContractActivateCommand(id));

            if (result.IsFailure)
                return BadRequest(new { error = result.Error, code = result.ErrorCode });

            return NoContent();
        }

        [HttpPatch("{id}/attach-document")]
        public async Task<IActionResult> AttachDocument(int id, [FromBody] AttachDocumentRequest request)
        {
            var result = await _mediator.Send(new AttachContractDocumentCommand(id, request.DocumentUrl));

            if (result.IsFailure)
                return BadRequest(new { error = result.Error, code = result.ErrorCode });

            return NoContent();
        }
    }

    public class AttachDocumentRequest
    {
        public string DocumentUrl { get; set; } = string.Empty;
    }
}