using MediatR;
using Microsoft.AspNetCore.Mvc;
using ContaCorrente.Application.UseCases.Transferencias.Commands.RealizarTransferencia;
using ContaCorrente.Api.Models.Response;
using ContaCorrente.Api.Models.Request;

namespace ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferenciaController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

   
    [HttpPost]
    [ProducesResponseType(typeof(RealizarTransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RealizarTransferencia(
        [FromBody] RealizarTransferenciaRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? chaveIdempotencia = null)
    {
        var command = new RealizarTransferenciaCommand
        {
            IdContaCorrenteOrigem = request.IdContaCorrenteOrigem,
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


