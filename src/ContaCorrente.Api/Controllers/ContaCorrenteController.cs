using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.InativarContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;
using ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterContaCorrente;
using ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterExtrato;
using ContaCorrente.Api.Models.Response;
using ContaCorrente.Api.Models.Request;

namespace ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaCorrenteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContaCorrenteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria uma nova conta corrente (Público - não requer autenticação)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CriarContaCorrenteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaCorrenteRequest request)
    {
        var command = new CriarContaCorrenteCommand
        {
            Numero = request.Numero,
            Cpf = request.Cpf,
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

    /// <summary>
    /// Obtém informações de uma conta corrente (Requer autenticação)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// Obtém o saldo da conta corrente (Requer autenticação)
    /// </summary>
    [HttpGet("saldo")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterSaldo()
    {
        // Obter ID da conta do token
        var idContaCorrente = User.Claims.FirstOrDefault(c => c.Type == "id_conta_corrente")?.Value;

        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });
        }

        var query = new ObterContaCorrenteQuery { IdContaCorrente = idContaCorrente };
        var result = await _mediator.Send(query);

        if (result.IsFailed)
        {
            return NotFound(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(new
        {
            numeroConta = result.Value.Numero,
            titular = result.Value.Nome,
            dataHoraConsulta = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            saldo = result.Value.Saldo
        });
    }

    /// <summary>
    /// Obtém o extrato da conta corrente (Requer autenticação)
    /// </summary>
    [HttpGet("extrato")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterExtrato(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        // Obter ID da conta do token
        var idContaCorrente = User.Claims.FirstOrDefault(c => c.Type == "id_conta_corrente")?.Value;

        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });
        }

        var query = new ObterExtratoQuery
        {
            IdContaCorrente = idContaCorrente,
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
    /// Realiza um depósito na conta corrente (Requer autenticação)
    /// </summary>
    [HttpPost("deposito")]
    [Authorize]
    [ProducesResponseType(typeof(RealizarDepositoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RealizarDeposito(
        [FromBody] RealizarDepositoRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? chaveIdempotencia = null)
    {
        // Obter ID da conta do token ou usar o fornecido
        var idContaCorrenteToken = User.Claims.FirstOrDefault(c => c.Type == "id_conta_corrente")?.Value;

        if (string.IsNullOrEmpty(idContaCorrenteToken))
        {
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });
        }

        // Se não fornecer conta, usa a do token
        var idContaCorrente = string.IsNullOrEmpty(request.IdContaCorrente)
            ? idContaCorrenteToken
            : request.IdContaCorrente;

        var command = new RealizarDepositoCommand
        {
            IdContaCorrente = idContaCorrente,
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
    /// Realiza um saque da conta corrente (Requer autenticação)
    /// </summary>
    [HttpPost("saque")]
    [Authorize]
    [ProducesResponseType(typeof(RealizarSaqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RealizarSaque(
        [FromBody] RealizarSaqueRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? chaveIdempotencia = null)
    {
        // Obter ID da conta do token
        var idContaCorrente = User.Claims.FirstOrDefault(c => c.Type == "id_conta_corrente")?.Value;

        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });
        }

        var command = new RealizarSaqueCommand
        {
            IdContaCorrente = idContaCorrente,
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

    /// <summary>
    /// Inativa uma conta corrente (Requer autenticação)
    /// </summary>
    [HttpPost("inativar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InativarConta([FromBody] InativarContaRequest request)
    {
        // Obter ID da conta do token
        var idContaCorrente = User.Claims.FirstOrDefault(c => c.Type == "id_conta_corrente")?.Value;

        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });
        }

        var command = new InativarContaCorrenteCommand
        {
            IdContaCorrente = idContaCorrente,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return NoContent();
    }
}

