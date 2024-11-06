using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace AspnetCoreMvcFull.Controllers
{

  public class RamaisController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentController> _logger;
    public RamaisController(IValidateSession validateSession, IConfiguration configuration,
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

    public async Task<IActionResult> Ramais()
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
        if (response.IsSuccessStatusCode)
        {
          var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioModel>();
          if (funcionario == null)
            return RedirectToAction("MiscError", "MiscError");

          UpdateSessionWithFuncionario(funcionario);

          string jsonFilePath =  Path.Combine(_webHostEnvironment.WebRootPath,"Ramais","ramais.json"); // Caminho do arquivo JSON

          string jsonString = await System.IO.File.ReadAllTextAsync(jsonFilePath);
          RamaisResponse ramais = new RamaisResponse();

          if (!string.IsNullOrEmpty(jsonString))
          {
            ramais = JsonSerializer.Deserialize<RamaisResponse>(jsonString);
          }

          var partialModel = new PartialModel
          {
            front = new FrontModel(UserIcon()),
            funcionario = funcionario,
            Ramais = ramais.Ramais,
          };

          return View("Ramais", partialModel);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching funcionario data");
        return View("MiscError", "MiscError");
      }

      return RedirectToAction("LoginBasic", "Auth");
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
    public class RamaisResponse
    {
      public List<RamaisModel> Ramais { get; set; }
    }
  }
}
