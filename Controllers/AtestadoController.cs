using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SkiaSharp;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace AspnetCoreMvcFull.Controllers
{

  public class AtestadoController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentController> _logger;
    public AtestadoController(IValidateSession validateSession, IConfiguration configuration,
                                  IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor,
                                  IHttpClientFactory httpClientFactory, ILogger<DocumentController> logger)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
      _httpClientFactory = httpClientFactory;
      _logger = logger;
    }

    public async Task<IActionResult> Atestado()
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      var client = _httpClientFactory.CreateClient();
      client.DefaultRequestHeaders.Authorization =
          new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

      var id = HttpContext.Session.GetInt32("Id") ?? -1;

      try
      {
        var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");

        if (!response.IsSuccessStatusCode)
        {
          _logger.LogWarning("Failed to retrieve Funcionario. Status code: {StatusCode}", response.StatusCode);
          return RedirectToAction("MiscError", "MiscError");
        }

        var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioModel>();
        if (funcionario == null)
        {
          _logger.LogWarning("Funcionario data is null for Id: {Id}", id);
          return RedirectToAction("MiscError", "MiscError");
        }

        UpdateSessionWithFuncionario(funcionario);

        response = await client.GetAsync($"http://localhost:5235/api/Atestados/atestados?OwnerId={funcionario.Id}");

        var atestados = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<AtestadoModel>>()
            : null;

        var partialModel = new PartialModel
        {
          front = new FrontModel(UserIcon()),
          funcionario = funcionario,
        };

        if (atestados != null)
        {
          partialModel.Atestados = atestados;
        }

        return View("Atestado", partialModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching Funcionario or Atestado data for Id: {Id}", id);
        return View("MiscError", "MiscError");
      }

    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AtestadoModel model, IFormFile Anexo)
    {
      if (!_validateSession.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      if (Anexo == null || Anexo.Length == 0)
      {
        return BadRequest("O anexo do atestado é obrigatório.");
      }

      // Extrair mês e ano de DataAtestado do modelo
      var dataAtestado = model.DataAtestado;
      var mesAnoPasta = dataAtestado.ToString("yyyy-MM");

      // Caminho base para salvar o arquivo do atestado, incluindo UserId
      var baseDirectory = GetUserAtestadoDirectory();
      var monthDirectory = Path.Combine(baseDirectory, mesAnoPasta);

      if (!Directory.Exists(monthDirectory))
      {
        Directory.CreateDirectory(monthDirectory);
      }

      // Salvar o arquivo anexo no diretório específico do mês
      var uploadExtension = Path.GetExtension(Anexo.FileName);
      if (string.IsNullOrEmpty(uploadExtension))
      {
        return BadRequest("O arquivo de upload não possui uma extensão.");
      }

      var fileNameWithExtension = Path.ChangeExtension(Guid.NewGuid().ToString(), uploadExtension);
      var filePath = Path.Combine(monthDirectory, fileNameWithExtension);

      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await Anexo.CopyToAsync(stream);
      }

      model.SetId();
      model.SetUrlAnexo("~/" + Path.GetRelativePath(_webHostEnvironment.ContentRootPath, filePath).Replace("\\", "/"));
      model.SetOwnerId(_validateSession.GetFuncionarioId());

      var response = await SendRegisterRequestAsync(model);
      return Ok("Atestado salvo com sucesso.");
    }

    private async Task<HttpResponseMessage> SendRegisterRequestAsync(AtestadoModel model)
    {
      using var client = CreateHttpClient();
      var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
      return await client.PostAsync("http://localhost:5235/api/Atestados/addatestado", content);
    }
    private HttpClient CreateHttpClient()
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      return new HttpClient(handler);
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
    // Função para obter o diretório base de atestados do usuário
    private string GetUserAtestadoDirectory() =>
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Users", _validateSession.GetUserId().ToString(), "Atestados");

    // Função auxiliar para obter o caminho formatado
    private string GetFormattedPathAtestado(string filePath, string baseDirectory)
    {
      baseDirectory = baseDirectory.Replace("\\", "/");
      filePath = filePath.Replace("\\", "/");

      if (filePath.StartsWith(baseDirectory))
      {
        string relativePath = filePath.Substring(baseDirectory.Length);

        if (relativePath.StartsWith("/"))
        {
          relativePath = relativePath.Substring(1);
        }

        int wwwrootIndex = baseDirectory.IndexOf("/wwwroot");

        string userDir = baseDirectory.Substring(wwwrootIndex + "/wwwroot".Length) + "/";

        return "~" + userDir + relativePath;
      }

      return filePath;
    }

    private void UpdateSessionWithFuncionario(FuncionarioModel funcionario)
    {
      _validateSession.SetFuncionarioId(funcionario.Id);
      _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());
    }
    private string UserIcon()
    {
      string patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      string key = _configuration["Authentication:Secret"];

      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId(), _validateSession.GetGenero());
    }

  }
}
