using LegalDocsPro.Application.Features.Contracts.Commands;
using LegalDocsPro.Application.Features.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Controllers
{
    [ApiController]
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
    }
}