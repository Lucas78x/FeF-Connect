using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using CpfCnpjLibrary;
using System.Text;
using Newtonsoft.Json;
using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.Enums;
using System.Net;
using AspnetCoreMvcFull.Utils.Token;

namespace AspnetCoreMvcFull.Controllers
{
  public class AuthController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly Serilog.ILogger _logger;
    private readonly IGenerateToken _generateToken;

    public AuthController(
        IValidateSession validateSession,
        IConfiguration configuration,
        IFileEncryptor fileEncryptor,
        Serilog.ILogger logger,
        IGenerateToken generateToken)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _fileEncryptor = fileEncryptor;
      _logger = logger;
      _generateToken = generateToken;
    }

    // Exibe a página de login básico
    public IActionResult LoginBasic()
    {
      return _validateSession.IsUserValid() ? RedirectToAction("Inicio", "Inicio") : View("LoginBasic", "Auth");
    }

    // Exibe a página de recuperação de senha
    public IActionResult ForgotPasswordBasic() => View();

    // Valida as credenciais de login
    public async Task<IActionResult> ValidateLogin(LoginModel login)
    {
      if (_validateSession.IsUserValid())
      {
        return RedirectToAction("Inicio", "Inicio");
      }

      if (!ModelState.IsValid)
      {
        HandleInvalidLoginModelState();
        return View("LoginBasic", "Auth");
      }

      var token = _generateToken.GenerateJwtToken(login.Username);
      var response = await SendLoginRequestAsync(login, token);

      return await HandleLoginResponseAsync(response, login);
    }

    private void HandleInvalidLoginModelState()
    {
      _logger.Error($"Dados de login inválidos: {ModelState}");
      ViewBag.ErrorMessage = "Dados de login inválidos. Por favor, verifique e tente novamente.";
    }

    private async Task<HttpResponseMessage> SendLoginRequestAsync(LoginModel login, string token)
    {
      using var client = CreateHttpClientWithToken(token);
      var requestBody = new
      {
        Id = login.Id,
        Username = login.Username,
        PasswordHash = login.PasswordHash
      };

      var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
      return await client.PostAsync("http://localhost:5235/api/Auth/login", content);
    }

    private HttpClient CreateHttpClientWithToken(string token)
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      var client = new HttpClient(handler);
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
      return client;
    }

    private async Task<IActionResult> HandleLoginResponseAsync(HttpResponseMessage response, LoginModel login)
    {
      if (response.IsSuccessStatusCode)
      {
        var loginResult = await DeserializeResponseAsync<LoginModel>(response);
        if (loginResult == null)
        {
          LogInvalidLoginResponse(login);
          return View("LoginBasic", "Auth");
        }

        _validateSession.SetSession((int)loginResult.Id, loginResult.Username, loginResult.Genero);
        _logger.Verbose($"Login realizado com sucesso para loginId {loginResult.Id}");
        return RedirectToAction("Inicio", "Inicio");
      }

      await HandleLoginErrorAsync(response, login);
      return View("LoginBasic", "Auth");
    }

    private async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
      var responseContent = await response.Content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<T>(responseContent);
    }

    private void LogInvalidLoginResponse(LoginModel login)
    {
      _logger.Error($"Erro de resposta para login {login}");
      ViewBag.ErrorMessage = "A resposta do servidor não é válida. Por favor, tente novamente.";
    }

    private async Task HandleLoginErrorAsync(HttpResponseMessage response, LoginModel login)
    {
      var errorContent = await response.Content.ReadAsStringAsync();
      switch (response.StatusCode)
      {
        case HttpStatusCode.NotFound:
          HandleNotFoundError(errorContent, login);
          break;
        case HttpStatusCode.BadRequest:
          ViewBag.ErrorMessage = "Dados de login inválidos. Por favor, revise e tente novamente.";
          break;
        case HttpStatusCode.InternalServerError:
          ViewBag.ErrorMessage = "Houve um problema no servidor. Tente novamente mais tarde.";
          break;
        default:
          ViewBag.ErrorMessage = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
          break;
      }
    }

    private void HandleNotFoundError(string errorContent, LoginModel login)
    {
      if (int.TryParse(errorContent, out int errorCode))
      {
        ViewBag.ErrorMessage = errorCode switch
        {
          (int)ErrorTypeEnum.Email => "O email informado não está registrado. Verifique e tente novamente.",
          (int)ErrorTypeEnum.Password => "A senha está incorreta. Verifique e tente novamente.",
          _ => "Usuário ou senha incorretos. Verifique e tente novamente."
        };
      }
      else
      {
        ViewBag.ErrorMessage = "Usuário ou senha incorretos. Verifique e tente novamente.";
      }
    }

    // Exibe a página de registro básico
    public IActionResult RegisterBasic() => View();

    // Registra um novo usuário
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
      if (!ModelState.IsValid || !Cpf.Validar(model.CPF))
      {
        ViewBag.ErrorMessage = "Dados de registro inválidos. Por favor, verifique e tente novamente.";
        return View("RegisterBasic", "Auth");
      }

      var response = await SendRegisterRequestAsync(model);
      ViewBag.SuccessMessage = response.IsSuccessStatusCode ? "Usuário cadastrado com sucesso!" : "Usuário já cadastrado";
      return View("RegisterBasic", "Auth");
    }

    private async Task<HttpResponseMessage> SendRegisterRequestAsync(RegisterModel model)
    {
      using var client = CreateHttpClient();
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

      var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
      return await client.PostAsync("http://localhost:5235/api/Auth/register", content);
    }

    private HttpClient CreateHttpClient()
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      return new HttpClient(handler);
    }

    // Faz o logout do usuário
    public IActionResult LogOut()
    {
      _validateSession.RemoveSession();
      return RedirectToAction("LoginBasic", "Auth");
    }
  }
}
