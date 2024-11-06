
namespace AspnetCoreMvcFull.Commands
{
  public class CriarMensagemCommand 
  {
    public Guid RemetenteId { get; set; }
    public Guid DestinatarioId { get; set; }
    public string Conteudo { get; set; }

    public CriarMensagemCommand(Guid remetenteId, Guid destinatarioId, string conteudo)
    {
      RemetenteId = remetenteId;
      DestinatarioId = destinatarioId;
      Conteudo = conteudo;
    }
  }

  public class AtualizarMensagemCommand 
  {
    public Guid MensagemId { get; set; }
    public string NovoConteudo { get; set; }

    public AtualizarMensagemCommand(Guid mensagemId, string novoConteudo)
    {
      MensagemId = mensagemId;
      NovoConteudo = novoConteudo;
    }
  }

  public class MarcarMensagemComoLidaCommand 
  {
    public Guid MensagemId { get; set; }

    public MarcarMensagemComoLidaCommand(Guid mensagemId)
    {
      MensagemId = mensagemId;
    }
  }

  public class DeletarMensagemCommand 
  {
    public Guid MensagemId { get; set; }

    public DeletarMensagemCommand(Guid mensagemId)
    {
      MensagemId = mensagemId;
    }
  }
}
