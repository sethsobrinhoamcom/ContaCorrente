using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContaCorrente.Application.UseCases.Auth.Commands.Login;

namespace ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Realiza login e retorna token JWT
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            CpfOrNumeroConta = request.CpfOrNumeroConta,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return Unauthorized(new { message = "Usuário ou senha inválidos", errorType = "USER_UNAUTHORIZED" });
        }

        return Ok(result.Value);
    }
}

// Request Model
public record LoginRequest
{
    public string CpfOrNumeroConta { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}