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

    public bool HasPermission(TipoPermissaoEnum permissão, params TipoPermissaoEnum[] roles) =>
    roles.Contains(permissão) || permissão == TipoPermissaoEnum.Administrador;

    public bool HasAdministrator(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Administrador, TipoPermissaoEnum.Gerente_Administrativo);

    public bool HasFinanceiro(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Financeiro);

    public bool HasAnalista(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Analista_De_Sistemas);

    public bool HasSuporte(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Suporte_Técnico);

    public bool HasComercial(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Comercial);

    public bool HasGerenteOperacional(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Gerente_Operacional);

    public bool HasGerenteComercial(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Gerente_Comercial);

    public bool HasTecnico(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Tecnico);

    public bool HasAuxiliarTecnico(TipoPermissaoEnum permissão) =>
        HasPermission(permissão, TipoPermissaoEnum.Auxiliar_Tecnico);


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
