using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Utils.PDF
{

  public interface IRelatorioService
  {
    byte[] GerarRelatorio(RequisicoesModel requisicao);
    byte[] GerarRelatorioFerias(FeriasModel ferias);
  }

}
