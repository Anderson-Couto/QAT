using QAT.Core.Models;
using QAT.Core.Models.Google;
using QAT.Core.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Moq.Protected;
using Microsoft.Extensions.Configuration;

namespace QAT.Tests
{
    [TestFixture]
    public class FreteServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private IConfiguration _configurationTest;

        [SetUp]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _configurationTest = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
        } 

        [Test]
        public void CalcularCustoEnvio_FreteNulo_ThrowsArgumentNullException()
        {
            // Arrange
            var freteService = new FreteService(_mockHttpClientFactory.Object, _configurationTest);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await freteService.CalcularCustoEnvio(null));
        }

        [TestCase("", "00000000")]
        [TestCase("00000000", "")]
        [TestCase("", "")]
        public void CalcularCustoEnvio_InvalidOrigemOuDestino_ThrowsArgumentException(string origem, string destino)
        {
            // Arrange
            var freteService = new FreteService(_mockHttpClientFactory.Object, _configurationTest);
            var frete = new Frete
            {
                Origem = origem,
                Destino = destino,
                Pacote = new Pacote
                {
                    PesoTotal = 10,
                    ValorTotal = 100
                }
            };

            // Act + Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await freteService.CalcularCustoEnvio(frete));
        }

        [Test]
        public async Task CalcularCustoEnvio_DeveRetornarCustoCorreto()
        {

            // Arrange
            var googleDistanceMatrix = new GoogleDistanceMatrix
            {
                rows = new List<Row>
                {
                        new Row
                        {
                            elements = new List<Element>
                            {
                                new Element
                                {
                                    distance = new ElementValue { value = 1000 },
                                    duration = new ElementValue { value = 1000 }
                                }
                            }
                        }
                    }
            };
            
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var mockHttpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        Content = JsonContent.Create(googleDistanceMatrix),
                        StatusCode = System.Net.HttpStatusCode.OK
                    })
                    .Verifiable();

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                            .Returns(mockHttpClient);

            var freteService = new FreteService(_mockHttpClientFactory.Object, _configurationTest);
            var origem = "Origem";
            var destino = "Destino";
            var pesoTotal = 10m;
            var pacote = new Pacote { PesoTotal = pesoTotal };
            var frete = new Frete { Origem = origem, Destino = destino, Pacote = pacote };

            // Act
            var result = await freteService.CalcularCustoEnvio(frete);

            // Assert
            Assert.That(result, Is.EqualTo(12.7m)); // 5m (valor base) + 10m (custo distância/tempo) + 0.5m (peso total * 0.05m) // O custo deve ser 15.85m (5m + 10m + 0.5m)
        }

        [Test]
        public async Task CalcularCustoEnvio_ComPacoteVazio_DeveRetornarCustoBase()
        {
            // Arrange
            var googleDistanceMatrix = new GoogleDistanceMatrix
            {
                rows = new List<Row>
                {
                        new Row
                        {
                            elements = new List<Element>
                            {
                                new Element
                                {
                                    distance = new ElementValue { value = 0 },
                                    duration = new ElementValue { value = 0 }
                                }
                            }
                        }
                    }
            };
            
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var mockHttpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        Content = JsonContent.Create(googleDistanceMatrix),
                        StatusCode = System.Net.HttpStatusCode.OK
                    })
                    .Verifiable();

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                            .Returns(mockHttpClient);

            var freteService = new FreteService(_mockHttpClientFactory.Object, _configurationTest);
            var origem = "Origem";
            var destino = "Destino";
            var pesoTotal = 0m;
            var pacote = new Pacote { PesoTotal = pesoTotal };
            var frete = new Frete { Origem = origem, Destino = destino, Pacote = pacote };

            // Act
            var custoEnvio = await freteService.CalcularCustoEnvio(frete);

            // Assert
            Assert.That(custoEnvio, Is.EqualTo(5m)); // O custo base deve ser 5m
        }
    }
}
