using LegalDocsPro.Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var userId = await _mediator.Send(command);

            // Devolvemos un 200 OK con el ID del nuevo usuario
            return Ok(new { Message = "Usuario registrado exitosamente", UserId = userId });
        }
    }
}