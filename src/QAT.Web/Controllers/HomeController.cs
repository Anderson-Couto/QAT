using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QAT.Core.Models;
using QAT.Core.Services;
using QAT.Web.Models;

namespace QAT.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFreteService _servicoFrete;

    public HomeController(ILogger<HomeController> logger, IFreteService servicoFrete)
    {
        _logger = logger;
        _servicoFrete = servicoFrete;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Pedido");
    }

    public IActionResult Pedido()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Pedido(Frete frete)
    {
        decimal custoEnvio = await _servicoFrete.CalcularCustoEnvio(frete);
        // Lógica para processar o pedido...
        decimal totalPedido = frete.Pacote.ValorTotal + custoEnvio;

        ViewBag.TotalPedido = totalPedido;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
