using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;

namespace AspnetCoreMvcFull.Controllers
{

  public class SitesController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentController> _logger;
    public SitesController(IValidateSession validateSession, IConfiguration configuration,
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

    public async Task<IActionResult> Sites()
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

          string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Sites", "Sites.json"); 

          string jsonString = await System.IO.File.ReadAllTextAsync(jsonFilePath);
          LinksModel Links = new ();

          if (!string.IsNullOrEmpty(jsonString))
          {
            Links = JsonConvert.DeserializeObject<LinksModel>(jsonString);
          }

          var partialModel = new PartialModel
          {
            front = new FrontModel(UserIcon()),
            funcionario = funcionario,
            Links = Links,
          };

          return View("Sites", partialModel);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching funcionario data");
        return View("MiscError", "MiscError");
      }

      return RedirectToAction("LoginBasic", "Auth");
    }

    [HttpPost]
    public async Task<IActionResult> AddLink(string linkName, string linkUrl, string linkDescription, string linkCategory, string newCategoryName)
    {

      if (string.IsNullOrWhiteSpace(linkName) || string.IsNullOrWhiteSpace(linkUrl))
      {
        TempData["ErrorMessage"] = "Nome e URL do link são obrigatórios.";
        return Json(new { success = false });
      }

      string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Sites", "Sites.json");
      string jsonString = await System.IO.File.ReadAllTextAsync(jsonFilePath);

      LinksModel linksModel = new LinksModel();

      if (!string.IsNullOrEmpty(jsonString))
      {
        linksModel = JsonConvert.DeserializeObject<LinksModel>(jsonString) ?? new LinksModel();
      }

      if (!string.IsNullOrWhiteSpace(newCategoryName))
      {
        linkCategory = newCategoryName;
      }

      var newLink = new Link
      {
        Nome = linkName,
        Url = linkUrl,
        Descricao = linkDescription
      };

      if (linksModel.Links.ContainsKey(linkCategory))
      {
        linksModel.Links[linkCategory].Add(newLink);
      }
      else
      {
        linksModel.Links[linkCategory] = new List<Link> { newLink };
      }

      jsonString = JsonConvert.SerializeObject(linksModel, Formatting.Indented);
      await System.IO.File.WriteAllTextAsync(jsonFilePath, jsonString);

      return Json(new { success = true });
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
