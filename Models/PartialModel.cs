using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Enums;

namespace AspnetCoreMvcFull.Models
{
  public class PartialModel
  {
    public FrontModel front { get; set; }
    public FrontModel UtilUrl { get; set; }
    public List<DocumentViewModel> Documents { get; set; }
    public List<FolderViewModel> Folders { get; set; }
    public List<RequisicoesModel> Requisicoes { get; set; }
    public FuncionarioModel funcionario { get; set; }
    public List<AtestadoModel> Atestados { get; set; }
    public List<FeriasModel> Ferias { get; set; }
    public List<RamaisModel> Ramais { get; set; }
    public ForecastModel Forecast { get; set; }
    public List<EventoModel> Eventos { get; set; }
    public DicasModel Dica { get; set; }
    public List<NoticiasModel> Noticias { get; set; }
    public List<ComunicadosModel> Comunicados { get; set; }
    public LinksModel Links { get; set; }
    public bool AdminUser(TipoPermissaoEnum permissao)
    {
      switch (permissao)
      {
        case TipoPermissaoEnum.Administrador:
        case TipoPermissaoEnum.Gerente_Administrativo:
        case TipoPermissaoEnum.Gerente_Operacional:
        case TipoPermissaoEnum.Gerente_Comercial:
        case TipoPermissaoEnum.SupervisorA:
        case TipoPermissaoEnum.SupervisorO:
        case TipoPermissaoEnum.SupervisorC:       
          return true;
        case TipoPermissaoEnum.Suporte_Técnico:
        case TipoPermissaoEnum.Financeiro:
        case TipoPermissaoEnum.Comercial:
        case TipoPermissaoEnum.Tecnico:
        case TipoPermissaoEnum.Auxiliar_Tecnico:
        case TipoPermissaoEnum.Tecnico_CFTV:
        case TipoPermissaoEnum.AuxiliarTCFTV:
        case TipoPermissaoEnum.Analista_De_Sistemas:
          return false;
      }

      return false;
    }

    //TODO: Refatorar 
    public List<FuncionarioModel> Funcionarios { get; set; }
    public List<PayslipModel> Payslips { get; set; }
    public DateTime? FilterDate { get; set; }
    public List<MensagemModel> Mensagens { get; set; }
    public Guid UsuarioAtualId { get; set; }
    public string NovoConteudo { get; set; }
    public Guid MensagemParaEditar { get; set; } // Para editar uma mensagem
    public Guid MensagemParaDeletar { get; set; } // Para deletar uma mensagem
  }
}
