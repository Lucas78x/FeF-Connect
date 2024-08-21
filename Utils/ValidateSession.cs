using AspnetCoreMvcFull.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Utils
{
  public interface IValidateSession
  {
    bool IsUserValid();
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
