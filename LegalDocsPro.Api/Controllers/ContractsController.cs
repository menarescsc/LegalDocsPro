using LegalDocsPro.Application.Features.Contracts.Commands;
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
    }
}