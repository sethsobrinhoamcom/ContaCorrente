using ContaCorrente.Api.Models.Request;
using ContaCorrente.Api.Models.Response;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;
using ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterExtrato;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaCorrenteController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

 

  
    [HttpPost]
    [ProducesResponseType(typeof(CriarContaCorrenteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaCorrenteRequest request)
    {
        var command = new CriarContaCorrenteCommand
        {
            Numero = request.Numero,
            Nome = request.Nome,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return CreatedAtAction(
            nameof(ObterConta),
            new { id = result.Value },
            new CriarContaCorrenteResponse { IdContaCorrente = result.Value });
    }

 
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterConta(string id)
    {
        var query = new ObterContaCorrenteQuery { IdContaCorrente = id };
        var result = await _mediator.Send(query);

        if (result.IsFailed)
        {
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtém o extrato da conta corrente
    /// </summary>
    [HttpGet("{id}/extrato")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterExtrato(
        string id,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        var query = new ObterExtratoQuery
        {
            IdContaCorrente = id,
            DataInicio = dataInicio,
            DataFim = dataFim
        };

        var result = await _mediator.Send(query);

        if (result.IsFailed)
        {
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(result.Value);
    }
    /// <summary>
    /// Realiza um depósito na conta corrente
    /// </summary>
    [HttpPost("{id}/deposito")]
    [ProducesResponseType(typeof(RealizarDepositoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RealizarDeposito(
        string id,
        [FromBody] RealizarDepositoRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? chaveIdempotencia = null)
    {
        var command = new RealizarDepositoCommand
        {
            IdContaCorrente = id,
            Valor = request.Valor,
            ChaveIdempotencia = chaveIdempotencia
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(new RealizarDepositoResponse
        {
            IdMovimento = result.Value,
            Mensagem = "Depósito realizado com sucesso"
        });
    }

    /// <summary>
    /// Realiza um saque da conta corrente
    /// </summary>
    [HttpPost("{id}/saque")]
    [ProducesResponseType(typeof(RealizarSaqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RealizarSaque(
        string id,
        [FromBody] RealizarSaqueRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? chaveIdempotencia = null)
    {
        var command = new RealizarSaqueCommand
        {
            IdContaCorrente = id,
            Valor = request.Valor,
            ChaveIdempotencia = chaveIdempotencia
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(new RealizarSaqueResponse
        {
            IdMovimento = result.Value,
            Mensagem = "Saque realizado com sucesso"
        });
    }

    // Request/Response Models
  

  

   

  
}

