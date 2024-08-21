using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Models
{
  public class PartialModel
  {
    public FrontModel front { get; set; }
    public FrontModel UtilUrl { get; set; }
    public List<DocumentViewModel> Documents { get; set; }
    public List<RequisicoesModel> Requisicoes { get; set; }
    public FuncionarioDTO funcionario { get; set; }
  }
}
