using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using CpfCnpjLibrary;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Enums;
using System.Net;
namespace AspnetCoreMvcFull.Controllers;

public class AuthController : Controller
{
  private readonly IValidateSession _validateSession;
  private readonly IConfiguration _configuration;
  private readonly IFileEncryptor _fileEncryptor;

  public AuthController(IValidateSession validateSession, IConfiguration configuration, IFileEncryptor fileEncryptor)
  {
    _validateSession = validateSession;
    _configuration = configuration;
    _fileEncryptor = fileEncryptor;
  }

  public IActionResult ForgotPasswordBasic() => View();
  public IActionResult LoginBasic()
  {

    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("Dashboard", "Dashboards");
    }
    else
    {
      return View("LoginBasic", "Auth");
    }
  }
  public async Task<IActionResult> ValidateLogin(LoginModel login)
  {
    if (ModelState.IsValid)
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      var client = new HttpClient(handler);

      var requestBody = new
      {
        Id = login.Id,
        Username = login.Username,
        PasswordHash = login.PasswordHash
      };

      var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

      try
      {

        var response = await client.PostAsync("http://localhost:5235/api/Auth/login", content);

        if (response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsStringAsync();

          var loginResult = JsonConvert.DeserializeObject<LoginModel>(responseContent);

          _validateSession.SetSession((int)loginResult.Id, loginResult.Username, loginResult.Genero);

          return RedirectToAction("Dashboard", "Dashboards");
        }
        else
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          if (response.StatusCode == HttpStatusCode.NotFound && int.TryParse(errorContent, out int errorCode))
          {
            if (errorCode == (int)ErrorTypeEnum.Email)
            {
              ViewBag.ErrorMessage = "Email invalido.";
            }
            else if (errorCode == (int)ErrorTypeEnum.Password)
            {
              ViewBag.ErrorMessage = "Senha invalida.";
            }
            else
            {
              ViewBag.ErrorMessage = "Usuario ou senha invalidos.";
            }
          }
          else
          {
            ViewBag.ErrorMessage = "Usuario ou senha invalidos.";
          }
        }
      }
      catch
      {

      }

    }

    return View("LoginBasic", "Auth");
  }
  [HttpPost]
  public async Task<IActionResult> Register([FromForm] RegisterModel model)
  {
    if (ModelState.IsValid)
    {
      if (Cpf.Validar(model.CPF))
      {
        var handler = new HttpClientHandler
        {
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        var client = new HttpClient(handler);

        var requestBody = new
        {
          Nome = model.Nome,
          Sobrenome = model.Sobrenome,
          CPF = model.CPF,
          RG = model.RG,
          Email = model.Email,
          Username = model.Username,
          PasswordHash = model.PasswordHash,
          Genero = model.Genero.GetHashCode(),
          DataNascimento = model.DataNascimento,
          Cargo = model.Cargo,
          Permissao = model.Permissao.GetHashCode()
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {

          var response = await client.PostAsync("https://localhost:7061/api/Auth/register", content);

          if (response.IsSuccessStatusCode)
          {
            ViewBag.SuccessMessage = "Usuário cadastrado com sucesso!";
          }
          else
          {
            ViewBag.ErrorMessage = "Usuário já cadastrado";

          }
        }
        catch
        {
          ViewBag.ErrorMessage = "Erro, tente novamente mais tarde";

        }
      }
      else
      {
        ViewBag.ErrorMessage = "CPF inválido.";
      }
    }
    else
    {
      ViewBag.ErrorMessage = "Erro, tente novamente mais tarde";
    }

    return View("RegisterBasic", "Auth");
  }
  public IActionResult RegisterBasic() => View();

  public IActionResult LogOut()
  {
    _validateSession.RemoveSession();

    return RedirectToAction("LoginBasic", "Auth");
  }
}
