namespace AspnetCoreMvcFull.DTO
{
  public class MensagemModel
  {
    public Guid Id { get; set; }
    public Guid RemetenteId { get; set; }
    public Guid DestinatarioId { get; set; }
    public string Conteudo { get; set; }
    public DateTime DataEnvio { get; set; }
    public bool Lida { get; set; }

    public FuncionarioModel Remetente { get; set; }
    public FuncionarioModel Destinatario { get; set; }
    public string RemetenteNome;

    public MensagemModel() { }
    
  }
}
