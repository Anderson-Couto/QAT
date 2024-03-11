using NUnit.Framework;
using QAT.Api.Controllers;
using QAT.Core.Models;
using QAT.Core.Services;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QAT.Tests
{
    [TestFixture]
    public class PedidosControllerTests
    {
        private PedidosController _controller;
        private Mock<ILogger<PedidosController>> _loggerMock;
        private Mock<IFreteService> _mockFreteService;

        [SetUp]
        public void Setup()
        {
            _mockFreteService = new Mock<IFreteService>();
            _loggerMock = new Mock<ILogger<PedidosController>>();
            _controller = new PedidosController(_loggerMock.Object, _mockFreteService.Object);
        }

        [Test]
        public async Task GetTotalPedido_ReturnsCorrectTotal()
        {
            // Arrange
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
            
            _mockFreteService.Setup(s => 
                s.CalcularCustoEnvio(It.IsAny<Frete>())
            ).ReturnsAsync(custoEnvio);

            // Act
            var okResult = await _controller.GetTotalPedidoAsync(frete);
            var result = okResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.That(result.Value, Is.EqualTo(frete.Pacote.ValorTotal + custoEnvio));
        }
    }
}
