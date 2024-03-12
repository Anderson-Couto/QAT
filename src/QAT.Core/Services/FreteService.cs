using System.Net.Http.Json;
using Newtonsoft.Json;
using QAT.Core.Models;
using QAT.Core.Models.Google;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;

namespace QAT.Core.Services;

public interface IFreteService
{
    Task<decimal> CalcularCustoEnvio(Frete frete);
}

public class FreteService : IFreteService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public FreteService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;

    }

    public async Task<decimal> CalcularCustoEnvio(Frete frete)
    {
        if (frete == null)
            throw new ArgumentNullException(nameof(frete));

        if (string.IsNullOrWhiteSpace(frete.Origem) || string.IsNullOrWhiteSpace(frete.Destino))
            throw new ArgumentException("Origem e destino devem ser fornecidos.");

        try
        {
            var distanceMatrix = await ObterMatrizDistancia(frete.Origem, frete.Destino);
            var custoDistanciaTempo = CalcularCustoDistanciaTempo(distanceMatrix);

            // Simulação de cálculo de custo de envio
            var calculo = 3m + custoDistanciaTempo + (frete.Pacote.PesoTotal * 0.5m);

            return Decimal.Round(calculo, 2);
        }
        catch (Exception ex)
        {
            // Tratar exceções conforme necessário
            throw new Exception("Ocorreu um erro ao calcular o custo de envio.", ex);
        }
    }

    private async Task<GoogleDistanceMatrix> ObterMatrizDistancia(string origem, string destino)
    {
        var apikey = _configuration.GetSection("GoogleApiKey").Get<string>();
        var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origem}&destinations={destino}&key={apikey}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Lança exceção se não for bem-sucedido


            return await response.Content.ReadFromJsonAsync<GoogleDistanceMatrix>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Erro ao acessar a API do Google Maps.", ex);
        }
    }

    private decimal CalcularCustoDistanciaTempo(GoogleDistanceMatrix distanceMatrix)
    {
        var custoDistanciaTempo = 0m;
        var elementsCount = 0;
        var taxaPorMetro = 0.001m;
        var taxaPorSegundo = 0.0017m;

        foreach (var row in distanceMatrix.rows)
        {
            foreach (var element in row.elements)
            {
                custoDistanciaTempo += element.distance.value * taxaPorMetro + element.duration.value * taxaPorSegundo;
                elementsCount++;
            }
        }

        return elementsCount == 0 ? 0m : custoDistanciaTempo / elementsCount;
    }
}