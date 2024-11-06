namespace AspnetCoreMvcFull.Models
{
  public class FeriasModel
  {
    public int Id { get; set; }
    public string NomeFuncionario { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Observacoes { get; set; }
    public Guid OwnerId { get; set; }

    public void SetNomeFuncionario(string Nome) => NomeFuncionario = Nome;
    public void SetOwnerId(Guid id) => OwnerId = id;
      
  }

}
