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
            // El controlador no sabe NADA de reglas de negocio ni de bases de datos.
            // Solo delega el comando a MediatR.
            var contractId = await _mediator.Send(command);

            return CreatedAtAction(nameof(CreateContract), new { id = contractId }, contractId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById(int id)
        {
            // Construimos la pregunta (Query)
            var query = new GetContractByIdQuery(id);

            // MediatR busca automáticamente al Handler para que haga el trabajo
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"No se encontró el contrato con el ID {id}.");

            return Ok(result);
        }
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] GetContractsWithPaginationQuery query)
        {
            // Gracias a [FromQuery], la API tomará los valores de la URL:
            // Ejemplo: /api/Contracts/paged?pageNumber=1&pageSize=5&searchTerm=arriendo
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPatch("{id}/send-to-review")]
        public async Task<IActionResult> SendToReview(int id)
        {
            await _mediator.Send(new SendContractToReviewCommand(id));
            return NoContent();
        }

        [HttpPatch("{id}/Approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _mediator.Send(new ContractApproveCommand(id));
            return NoContent();
        }

        [HttpPatch("{id}/Activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _mediator.Send(new ContractActivateCommand(id));
            return NoContent();
        }
        [HttpPatch("{id}/attach-document")]
        public async Task<IActionResult> AttachDocument(int id, [FromBody] AttachDocumentRequest request)
        {
            await _mediator.Send(new AttachContractDocumentCommand(id, request.DocumentUrl));
            return NoContent();
        }
    }

    // Añade esto fuera de tu controlador o dentro de tu namespace
    public class AttachDocumentRequest
    {
        public string DocumentUrl { get; set; } = string.Empty;
    }
}