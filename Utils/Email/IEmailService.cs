using AspnetCoreMvcFull.Enums;

namespace AspnetCoreMvcFull.Utils.Email
{
  public interface IEmailService
  {
    void EnviarEmail(string requisitanteNome, string requisicao, string requisitanteEmail, string relato, TipoStatusEnum status);
  }
}
