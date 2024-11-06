namespace AspnetCoreMvcFull.Models
{
  public class ForecastModel
  {
    public string Temperatura { get; set; }
    public string Descricao { get; set; }
    public string Umidade { get; set; }
    public string Chuva { get; set; }

    public ForecastModel(string temperatura, string descricao, string umidade, string chuva)
    {
      Temperatura = temperatura;
      Descricao = descricao;
      Umidade = umidade;
      Chuva = chuva;
    }
    public ForecastModel()
    {

    }
  }
}
