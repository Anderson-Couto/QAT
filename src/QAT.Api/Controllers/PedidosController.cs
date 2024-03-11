using Microsoft.AspNetCore.Mvc;
using QAT.Core.Models;
using QAT.Core.Services;

namespace QAT.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PedidosController : ControllerBase
{
    private readonly ILogger<PedidosController> _logger;
    private readonly IFreteService _servicoFrete;

    public PedidosController(ILogger<PedidosController> logger, IFreteService servicoFrete)
    {
        _logger = logger;
        _servicoFrete = servicoFrete;
    }

    // GET: api/TodoItems
    [HttpPost("/totalpedido")]
    public async Task<ActionResult<decimal>> GetTotalPedidoAsync([FromBody] Frete frete)
    {

        decimal custoEnvio = await _servicoFrete.CalcularCustoEnvio(frete);
        // Lógica para processar o pedido...
        decimal totalPedido = frete.Pacote.ValorTotal + custoEnvio;

        return Ok(totalPedido);
    }
}