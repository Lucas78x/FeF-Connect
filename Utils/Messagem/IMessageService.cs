using AspnetCoreMvcFull.Commands;
using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Utils.Messagem
{
  public interface IMessageService
  {
    Task<List<MensagemModel>> ObterMensagensAsync();
    Task<Guid?> EnviarMensagemAsync(CriarMensagemCommand command);
    Task<bool> AtualizarMensagemAsync(AtualizarMensagemCommand command);
    Task<bool> MarcarComoLidaAsync(MarcarMensagemComoLidaCommand command);
    Task<bool> DeletarMensagemAsync(Guid mensagemId);
  }
}
