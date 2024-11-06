using AspnetCoreMvcFull.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;

namespace AspnetCoreMvcFull.Utils
{
  public interface IValidateSession
  {
    bool IsUserValid();
    bool HasAdministrator(TipoPermissaoEnum permissão);
    bool HasFinanceiro(TipoPermissaoEnum permissão);
    bool HasAnalista(TipoPermissaoEnum permissão);
    bool HasSuporte(TipoPermissaoEnum permissão);
    bool HasComercial(TipoPermissaoEnum permissão);
    bool HasGerenteOperacional(TipoPermissaoEnum permissão);
    bool HasGerenteComercial(TipoPermissaoEnum permissão);
    bool HasTecnico(TipoPermissaoEnum permissão);
    bool HasAuxiliarTecnico(TipoPermissaoEnum permissão);

    void RemoveSession();
    string GetUsername();
    int GetUserId();
    Guid GetFuncionarioId();
    TipoGeneroEnum GetGenero();
    TipoPermissaoEnum GetPermissao();

    void SetFuncionarioPermissao(int Permissao);
    void SetFuncionarioId(Guid id);
    void SetSession(int Id, string Username, int Genero);
  }

  public class ValidateSession : IValidateSession
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ValidateSession(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public bool IsUserValid()
    {
      return _httpContextAccessor.HttpContext.Session.GetInt32("Id") != null;
    }
    public string GetUsername()
    {
      return _httpContextAccessor.HttpContext.Session.GetString("Username");
    }

    public bool HasAdministrator(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Administrador || permissão == TipoPermissaoEnum.Gerente_Administrativo;
    public bool HasFinanceiro(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Financeiro || permissão == TipoPermissaoEnum.Administrador;

    public bool HasAnalista(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Analista_De_Sistemas || permissão == TipoPermissaoEnum.Administrador;

    public bool HasSuporte(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Suporte_Técnico || permissão == TipoPermissaoEnum.Administrador;

    public bool HasComercial(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Comercial || permissão == TipoPermissaoEnum.Administrador;

    public bool HasGerenteOperacional(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Gerente_Operacional || permissão == TipoPermissaoEnum.Administrador;

    public bool HasGerenteComercial(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Gerente_Comercial || permissão == TipoPermissaoEnum.Administrador;

    public bool HasTecnico(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Tecnico || permissão == TipoPermissaoEnum.Administrador;

    public bool HasAuxiliarTecnico(TipoPermissaoEnum permissão) => permissão == TipoPermissaoEnum.Auxiliar_Tecnico|| permissão == TipoPermissaoEnum.Administrador;

    public int GetUserId()
    {
      return _httpContextAccessor.HttpContext.Session.GetInt32("Id") ?? 0;
    }
    public Guid GetFuncionarioId()
    {
      string funcionarioString = _httpContextAccessor.HttpContext.Session.GetString("FuncionarioId");

      return Guid.Parse(funcionarioString);
    }
    public TipoGeneroEnum GetGenero()
    {
      var genero = _httpContextAccessor.HttpContext.Session.GetInt32("Genero") ?? 0;

      return (TipoGeneroEnum)genero;
    }
    public TipoPermissaoEnum GetPermissao()
    {
      var permissao = _httpContextAccessor.HttpContext.Session.GetInt32("Permissao") ?? 0;

      return (TipoPermissaoEnum)permissao;
    }

    public void SetSession(int Id,string Username, int Genero)
    {
      _httpContextAccessor.HttpContext.Session.SetInt32("Id",Id);
      _httpContextAccessor.HttpContext.Session.SetString("Username", Username);
      _httpContextAccessor.HttpContext.Session.SetInt32("Genero", Genero);
    }
    public void SetFuncionarioId(Guid id)
    {
      _httpContextAccessor.HttpContext.Session.SetString("FuncionarioId", id.ToString());
    }
    public void SetFuncionarioPermissao(int Permissao)
    {
      _httpContextAccessor.HttpContext.Session.SetInt32("Permissao", Permissao);
    }
    public void RemoveSession() => _httpContextAccessor.HttpContext.Session.Clear();
  }
}
