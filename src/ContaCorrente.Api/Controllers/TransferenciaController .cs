using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContaCorrente.Application.UseCases.Transferencias.Commands.RealizarTransferencia;
using ContaCorrente.Api.Models.Response;
using ContaCorrente.Api.Models.Request;

namespace ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticação em todos os endpoints
public class TransferenciaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferenciaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Realiza uma transferência entre contas (Requer autenticação)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RealizarTransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RealizarTransferencia(
        [FromBody] RealizarTransferenciaRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? chaveIdempotencia = null)
    {
        // Obter ID da conta de origem do token
        var idContaCorrenteOrigem = User.Claims.FirstOrDefault(c => c.Type == "id_conta_corrente")?.Value;

        if (string.IsNullOrEmpty(idContaCorrenteOrigem))
        {
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });
        }

        var command = new RealizarTransferenciaCommand
        {
            IdContaCorrenteOrigem = idContaCorrenteOrigem,
            IdContaCorrenteDestino = request.IdContaCorrenteDestino,
            Valor = request.Valor,
            ChaveIdempotencia = chaveIdempotencia
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(new RealizarTransferenciaResponse
        {
            IdTransferencia = result.Value,
            Mensagem = "Transferência realizada com sucesso"
        });
    }
}
