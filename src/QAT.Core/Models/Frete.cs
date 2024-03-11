namespace QAT.Core.Models;

public class Frete
{
    public required string Origem { get; set; }
    public required string Destino { get; set; }
    public required Pacote Pacote { get; set; }
}