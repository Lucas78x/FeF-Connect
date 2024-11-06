namespace AspnetCoreMvcFull.Models
{
  public class AtestadoModel
  {
    public Guid Id { get; set; }
    public DateTime DataAtestado { get; set; }
    public string Motivo { get; set; }
    public string UrlAnexo { get; set; } // URL do PDF
    public Guid ownerId { get; set; }


    public void SetId() => Id = Guid.NewGuid();
    public void SetOwnerId(Guid OwnerId) => ownerId = OwnerId;
    public void SetUrlAnexo(string Anexo) => UrlAnexo = Anexo;
  }
}
