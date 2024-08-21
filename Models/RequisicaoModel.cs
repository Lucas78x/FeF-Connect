namespace AspnetCoreMvcFull.Models
{
  public class RequisicaoModel
  {
    public Guid Id { get; set; }
    public string Requisitante { get; set; }
    public string Descricao { get; set; }
    public string DescricaoSolucao { get; set; }
    public string Status { get; set; }
    public DateTime? DataFinalizacao { get; set; }
  }

}
