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
  private readonly Serilog.ILogger _logger;
  public AuthController(IValidateSession validateSession, IConfiguration configuration, IFileEncryptor fileEncryptor, Serilog.ILogger logger)
  {
    _validateSession = validateSession;
    _configuration = configuration;
    _fileEncryptor = fileEncryptor;
    _logger = logger;
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
  public async Task<IActionResult> ValidateLogin(Models.LoginModel login)
  {
    if (_validateSession.IsUserValid())
    {

      return RedirectToAction("Dashboard", "Dashboards");
    }

    if (!ModelState.IsValid)
    {
      _logger.Error($"Dados de login inválidos{ModelState}");
      ViewBag.ErrorMessage = "Dados de login inválidos. Por favor, verifique e tente novamente.";
      return View("LoginBasic", "Auth");
    }

    var handler = new HttpClientHandler
    {
      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };

    using (var client = new HttpClient(handler))
    {
      var requestBody = new
      {
        Id = login.Id,
        Username = login.Username,
        PasswordHash = login.PasswordHash
      };

      var content = new StringContent(
          System.Text.Json.JsonSerializer.Serialize(requestBody),
          Encoding.UTF8,
          "application/json");

      try
      {
        var response = await client.PostAsync("http://localhost:5235/api/Auth/login", content);

        if (response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsStringAsync();

          var loginResult = JsonConvert.DeserializeObject<Models.LoginModel>(responseContent);

          if (loginResult == null)
          {
            _logger.Error($"Erro de resposta para login {login}");
            ViewBag.ErrorMessage = "A resposta do servidor não é válida. Por favor, tente novamente.";
            return View("LoginBasic", "Auth");
          }

          _validateSession.SetSession((int)loginResult.Id, loginResult.Username, loginResult.Genero);
          _logger.Verbose($"login realizado com sucesso para loginId {loginResult.Id}");
          return RedirectToAction("Dashboard", "Dashboards");
        }
        else
        {
          // Lê a resposta de erro
          var errorContent = await response.Content.ReadAsStringAsync();

          if (response.StatusCode == HttpStatusCode.NotFound)
          {
            if (int.TryParse(errorContent, out int errorCode))
            {
              switch (errorCode)
              {
                case (int)ErrorTypeEnum.Email:
                  ViewBag.ErrorMessage = "O email informado não está registrado. Verifique e tente novamente.";
                  break;
                case (int)ErrorTypeEnum.Password:
                  ViewBag.ErrorMessage = "A senha está incorreta. Verifique e tente novamente.";
                  break;
                default:
                  _logger.Error($"Erro ao fazer login, senha ou login incorreto para login{login}");
                  ViewBag.ErrorMessage = "Usuário ou senha incorretos. Verifique e tente novamente.";
                  break;
              }
            }
            else
            {
              ViewBag.ErrorMessage = "Usuário ou senha incorretos. Verifique e tente novamente.";
            }
          }
          else if (response.StatusCode == HttpStatusCode.BadRequest)
          {
            ViewBag.ErrorMessage = "Dados de login inválidos. Por favor, revise e tente novamente.";
          }
          else if (response.StatusCode == HttpStatusCode.InternalServerError)
          {
            ViewBag.ErrorMessage = "Houve um problema no servidor. Tente novamente mais tarde.";
          }
          else
          {
            ViewBag.ErrorMessage = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
          }
        }
      }
      catch (HttpRequestException ex)
      {
        ViewBag.ErrorMessage = $"Houve um problema ao tentar se conectar: {ex.Message}. Por favor, tente novamente.";
      }
      catch (Exception ex)
      {
        ViewBag.ErrorMessage = $"Ocorreu um erro inesperado: {ex.Message}. Por favor, tente novamente mais tarde.";
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
