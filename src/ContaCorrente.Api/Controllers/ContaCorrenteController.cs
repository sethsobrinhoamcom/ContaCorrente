using MediatR;
using Microsoft.AspNetCore.Mvc;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterExtrato;
using ContaCorrente.Api.Models.Response;
using ContaCorrente.Api.Models.Request;

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
}

