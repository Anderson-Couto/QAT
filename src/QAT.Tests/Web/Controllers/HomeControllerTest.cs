using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using QAT.Core.Models;
using QAT.Core.Services;
using QAT.Web.Controllers;

namespace QAT.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public void Index_RedirecionaParaPedido()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var freteServiceMock = new Mock<IFreteService>();
            var controller = new HomeController(loggerMock.Object, freteServiceMock.Object);

            // Act
            var result = controller.Index() as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Pedido"));
        }

        [Test]
        public async Task Pedido_Post_ReturnaViewComTotalPedidoNaViewBag()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var freteServiceMock = new Mock<IFreteService>();
            var controller = new HomeController(loggerMock.Object, freteServiceMock.Object);

            var frete = new Frete
            {
                Origem = "Origem",
                Destino = "Destino",
                Pacote = new Pacote { 
                    PesoTotal = 10, 
                    ValorTotal = 100 
                }
            };

            decimal custoEnvio = 50; // Custo de envio esperado
            freteServiceMock.Setup(
                service => service.CalcularCustoEnvio(frete)
            ).ReturnsAsync(custoEnvio);

            // Act
            var result = await controller.Pedido(frete) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ContainsKey("TotalPedido"));
            Assert.That(result.ViewData["TotalPedido"], Is.EqualTo(frete.Pacote.ValorTotal + custoEnvio));
        }
    }
}
