using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Controllers
{
  public class DocumentController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    public DocumentController(IValidateSession validateSession, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
    }

    public async Task<IActionResult> Document()
    {
      if (_validateSession.IsUserValid())
      {
        var handler = new HttpClientHandler
        {
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        var client = new HttpClient(handler);
        var id = HttpContext.Session.GetInt32("Id") ?? -1;

        try
        {

          client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

          var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");
          if (response.IsSuccessStatusCode)
          {
            var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioDTO>();
            if (funcionario != null)
            {
              _validateSession.SetFuncionarioId(funcionario.Id);
              _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());
              var frontModel = new FrontModel(UserIcon());
              var partialModel = new PartialModel();
              partialModel.front = frontModel;
              partialModel.funcionario = funcionario;

              partialModel.Documents = GetDocumentsFromDirectory();
              return View("Document", partialModel);
            }
            else
            {

              return RedirectToPage("MiscError", "Pages");
            }
          }
        }
        catch
        {
          return View("MiscError", "Pages");

        }
      }
      else
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      return RedirectToAction("LoginBasic", "Auth");
    }
    public List<DocumentViewModel> GetDocumentsFromDirectory()
    {

      var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{_validateSession.GetPermissao().GetHashCode()}/Document");

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

          documents.Add(new DocumentViewModel
          {
            Title = title,
            Category = category,
            FilePath = $"/{_validateSession.GetPermissao().GetHashCode()}/Document/{Path.GetFileName(file)}",
            FileType = fileExtension,
            ImagePath = "/images/default.png"
          });
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
    public IActionResult RedirecToDocument()
    {
      if (_validateSession.IsUserValid())
      {
        return RedirectToAction("Document", "Document");
      }
      else
      {
        return RedirectToAction("LoginBasic", "Auth");
      }
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, string title, string fileType)
    {
      if (file != null && file.Length > 0)
      {
        var fileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

        // Validação básica para garantir que o tipo de arquivo seja suportado
        var allowedExtensions = new[] { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "mp4", "txt" };
        if (!allowedExtensions.Contains(fileExtension))
        {
          return Json(new { success = false, message = "Tipo de arquivo não suportado." });
        }

        var fileName = $"{title}_{fileType}_{Guid.NewGuid()}.{fileExtension}";

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{_validateSession.GetPermissao().GetHashCode()}/Document");

        if (!Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }

        var filePath = Path.Combine(directoryPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        // Aqui você pode adicionar código para salvar as informações do documento no banco de dados, se necessário.

        // Retornar um JSON indicando sucesso
        return Json(new { success = true });
      }

      // Caso o arquivo seja nulo ou vazio, retornar uma resposta indicando falha
      return Json(new { success = false, message = "Nenhum arquivo enviado." });
    }

    [HttpPost]
    public IActionResult RemoveDocument([FromBody] RemoveDocumentRequest request)
    {
      if (request != null)
      {
        var basePath = _webHostEnvironment.WebRootPath;

        // Caminho do arquivo solicitado (já deve estar no formato correto, sem necessidade de conversão)
        var filePath = request.FilePath.Replace("/","\\");

        // Combine o caminho base com o caminho do arquivo
        var file = basePath+filePath;

        System.IO.File.Delete(file);
        return Json(new { success = true });
      }
      else
      {
        return Json(new { success = false });
      }
    }

    public class RemoveDocumentRequest
    {
      public string FilePath { get; set; }
    }
  }
}


