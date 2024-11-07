using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTO;
using Microsoft.Extensions.Logging;
using Ganss.Xss;

namespace AspnetCoreMvcFull.Controllers
{
  public class NocDocumentController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NocDocumentController> _logger;

    public NocDocumentController(IValidateSession validateSession, IConfiguration configuration,
                              IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor,
                              IHttpClientFactory httpClientFactory, ILogger<NocDocumentController> logger)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
      _httpClientFactory = httpClientFactory;
      _logger = logger;
    }

    public async Task<IActionResult> NocDocument()
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");


      if (!_validateSession.HasAnalista(_validateSession.GetPermissao()) || !_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

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

          var partialModel = new PartialModel
          {
            front = new FrontModel(UserIcon()),
            funcionario = funcionario,
            Documents = GetDocumentsFromDirectory()
          };

          return View("NocDocument", partialModel);
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

    public List<DocumentViewModel> GetDocumentsFromDirectory()
    {
      var documentList = new List<DocumentViewModel>();

      try
      {
        var directoryPath = Path.Combine(_webHostEnvironment.WebRootPath,
                                         _validateSession.GetPermissao().GetHashCode().ToString(),
                                         "Document");

        if (_validateSession.HasAdministrator(_validateSession.GetPermissao()))
          directoryPath = Path.Combine(_webHostEnvironment.WebRootPath,
                                         TipoPermissaoEnum.Analista_De_Sistemas.GetHashCode().ToString(),
                                         "Document"); ;


        if (!Directory.Exists(directoryPath))
          return documentList;

        var files = Directory.GetFiles(directoryPath)
                             .OrderByDescending(file => System.IO.File.GetCreationTime(file))
                             .ToArray();

        documentList = files.Select(file =>
        {
          try
          {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var fileExtension = Path.GetExtension(file).TrimStart('.');
            var parts = fileName.Split('_');

            if (parts.Length >= 3)
            {
              return new DocumentViewModel
              {
                Title = parts[0],
                Category = parts[1],
                FilePath = _validateSession.HasAdministrator(_validateSession.GetPermissao())
                      ? $"/{TipoPermissaoEnum.Analista_De_Sistemas.GetHashCode()}/Document/{Path.GetFileName(file)}"
                      : $"/{_validateSession.GetPermissao().GetHashCode()}/Document/{Path.GetFileName(file)}",
                FileType = fileExtension,
                ImagePath = "/images/default.png"
              };
            }
            return null;
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Erro ao processar o arquivo {file}: {ex.Message}");
            return null;
          }
        }).Where(doc => doc != null).ToList();
      }
      catch (DirectoryNotFoundException ex)
      {
        Console.WriteLine($"Diretório não encontrado: {ex.Message}");
      }
      catch (UnauthorizedAccessException ex)
      {
        Console.WriteLine($"Acesso não autorizado: {ex.Message}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro inesperado: {ex.Message}");
      }

      return documentList;
    }

    private string UserIcon()
    {
      string patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      string key = _configuration["Authentication:Secret"];

      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId(), _validateSession.GetGenero());
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, string title, string fileType)
    {
      var sanitizer = new HtmlSanitizer();
      title = sanitizer.Sanitize(title);

      if (file == null || file.Length == 0)
        return Json(new { success = false, message = "Nenhum arquivo enviado." });

      if (!_validateSession.HasAnalista(_validateSession.GetPermissao()) || !_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

      var fileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
      var allowedExtensions = new[] { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "mp4", "txt" };

      if (!allowedExtensions.Contains(fileExtension))
        return Json(new { success = false, message = "Tipo de arquivo não suportado." });

      var fileName = $"{title}_{fileType}_{Guid.NewGuid()}.{fileExtension}";
      var directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, _validateSession.GetPermissao().GetHashCode().ToString(), "Document");

      if (_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, TipoPermissaoEnum.Financeiro.GetHashCode().ToString(), "Document");

      if (!Directory.Exists(directoryPath))
        Directory.CreateDirectory(directoryPath);

      var filePath = Path.Combine(directoryPath, fileName);

      try
      {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        return Json(new { success = true });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error uploading file");
        return Json(new { success = false, message = "Erro ao fazer upload do arquivo." });
      }
    }

    [HttpPost]
    public IActionResult RemoveDocument([FromBody] RemoveDocumentRequest request)
    {
      if (request == null)
        return Json(new { success = false });

      if (!_validateSession.HasAnalista(_validateSession.GetPermissao()) || !_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

      var filePath = Path.Combine(_webHostEnvironment.WebRootPath, request.FilePath.TrimStart('/').Replace("/", "\\"));
      if (System.IO.File.Exists(filePath))
      {
        System.IO.File.Delete(filePath);
        return Json(new { success = true });
      }

      return Json(new { success = false, message = "Arquivo não encontrado." });
    }
    public class RemoveDocumentRequest
    {
      public string FilePath { get; set; }
    }
  }
}
