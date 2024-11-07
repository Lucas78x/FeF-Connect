using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Controllers
{
  public class NocTutorialController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    public NocTutorialController(IValidateSession validateSession, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
    }

    public async Task<IActionResult> NocTutorial()
    {
      if (!_validateSession.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      if (!_validateSession.HasAnalista(_validateSession.GetPermissao()) || !_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

      var id = HttpContext.Session.GetInt32("Id") ?? -1;
      if (id == -1)
      {
        return RedirectToAction("MiscError", "MiscError");
      }

      try
      {
        using var handler = new HttpClientHandler
        {
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

        var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");
        if (!response.IsSuccessStatusCode)
        {
          return RedirectToAction("MiscError", "MiscError");
        }

        var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioModel>();
        if (funcionario == null)
        {
          return RedirectToAction("MiscError", "MiscError");
        }

        _validateSession.SetFuncionarioId(funcionario.Id);
        _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());

        var frontModel = new FrontModel(UserIcon());
        var partialModel = new PartialModel
        {
          front = frontModel,
          funcionario = funcionario,
          Documents = GetDocumentsFromDirectory()
        };

        return View("NocTutorial", partialModel);
      }
      catch
      {
        return View("MiscError", "MiscError");
      }
    }

    public List<DocumentViewModel> GetDocumentsFromDirectory()
    {

      var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{_validateSession.GetPermissao().GetHashCode()}");

      if (_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{TipoPermissaoEnum.Analista_De_Sistemas.GetHashCode()}");

      if (!Directory.Exists(directoryPath))
      {
        return new List<DocumentViewModel>();
      }

      var files = Directory.GetFiles(directoryPath)
                        .OrderByDescending(file => System.IO.File.GetCreationTime(file))
                        .ToArray();

      var documents = new List<DocumentViewModel>();

      foreach (var file in files)
      {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var fileExtension = Path.GetExtension(file).TrimStart('.');

        var parts = fileName.Split('_');

        if (parts.Length >= 3)
        {
          var title = parts[0];
          var category = parts[1];

          if (!_validateSession.HasAdministrator(_validateSession.GetPermissao()))
          {
            documents.Add(new DocumentViewModel
            {
              Title = title,
              Category = category,
              FilePath = $"/{_validateSession.GetPermissao().GetHashCode()}/{Path.GetFileName(file)}",
              FileType = fileExtension,
              ImagePath = "/images/default.png"
            });
          }
          else
          {
            documents.Add(new DocumentViewModel
            {
              Title = title,
              Category = category,
              FilePath = $"/{TipoPermissaoEnum.Analista_De_Sistemas.GetHashCode()}/{Path.GetFileName(file)}",
              FileType = fileExtension,
              ImagePath = "/images/default.png"
            });
          }
        }
      }

      return documents;
    }

    private string UserIcon()
    {
      string patch = "";
      string key = "";
      patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      key = _configuration["Authentication:Secret"];

      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId(), _validateSession.GetGenero());
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, string title, string category)
    {
      if (file != null && file.Length > 0)
      {
        if (!_validateSession.HasAnalista(_validateSession.GetPermissao()) || !_validateSession.HasAdministrator(_validateSession.GetPermissao()))
          return RedirectToAction("MiscError", "MiscError");

        var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
        var fileName = $"{title}_{category}_{Guid.NewGuid()}.{fileExtension}";

        if (category == "backup")
        {
          fileName = $"{title}_{category}.{fileExtension}";
        }

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{_validateSession.GetPermissao().GetHashCode()}");

        if (_validateSession.HasAdministrator(_validateSession.GetPermissao())) directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{TipoPermissaoEnum.Analista_De_Sistemas.GetHashCode()}");

        if (!Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }

        var filePath = Path.Combine(directoryPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        return Json(new { success = true });
      }

      return Json(new { success = false });
    }
  }
}


