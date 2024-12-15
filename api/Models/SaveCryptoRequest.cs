namespace api.Models;

public class SaveCryptoRequest
{
    public required decimal Price { get; set; }
    public required decimal Open { get; set; }
    public required decimal Change { get; set; }
    public required decimal ChangePercent { get; set; }
    public required decimal Volume { get; set; }
    public required decimal High24h { get; set; }
    public required decimal Low24h { get; set; }
}