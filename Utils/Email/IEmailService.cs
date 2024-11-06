using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Utils.Email
{
  public interface IEmailService
  {
    void EnviarEmail(string requisitanteNome, string requisicao, string requisitanteEmail, string relato, TipoStatusEnum status, byte[] pdfBytes);
    void EnviarEmailFeriasProgramadas(FeriasModel ferias, string funcionarioEmail, byte[] pdfBytes);
  }
}
