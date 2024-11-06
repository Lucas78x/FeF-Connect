using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTO;

public class RelatoriosController : Controller
{
  private readonly IValidateSession _validateSession;
  private readonly IConfiguration _configuration;
  private readonly IWebHostEnvironment _webHostEnvironment;
  private readonly IFileEncryptor _fileEncryptor;
  private readonly IRelatorioService _relatorioService;

  public RelatoriosController(
      IValidateSession validateSession,
      IConfiguration configuration,
      IWebHostEnvironment webHostEnvironment,
      IFileEncryptor fileEncryptor,
      IRelatorioService relatorioService)
  {
    _validateSession = validateSession;
    _configuration = configuration;
    _webHostEnvironment = webHostEnvironment;
    _fileEncryptor = fileEncryptor;
    _relatorioService = relatorioService;
  }

  public async Task<IActionResult> Relatorios()
  {
    if (!IsUserValid()) return RedirectToLogin();

    int userId = GetUserIdFromSession();
    if (userId == -1) return RedirectToErrorPage();

    try
    {
      var funcionario = await GetFuncionarioAsync(userId);
      if (funcionario == null) return RedirectToErrorPage();

      SetFuncionarioDataInSession(funcionario);

      var model = CreatePartialModel(funcionario);
      return View("Relatorios", model);
    }
    catch
    {
      return View("MiscError", "MiscError");
    }
  }

  [HttpPost]
  public IActionResult AddFile(string folderName, string fileName, IFormFile fileUpload)
  {
    if (!IsUserValid()) return RedirectToLogin();

    if (!IsValidFile(fileUpload)) return Json(new { success = false });

    try
    {
      SaveFile(folderName, fileName, fileUpload);
      return Json(new { success = true });
    }
    catch
    {
      return Json(new { success = false });
    }
  }

  public IActionResult AddFolder(string folderName)
  {
    if (!IsUserValid()) return RedirectToLogin();

    try
    {
      CreateFolder(folderName);
      return Json(new { success = true });
    }
    catch (Exception ex)
    {
      return Json(new { success = false });
    }
  }

  [HttpPost]
  public IActionResult DeleteFolder(string folderName)
  {
    if (!IsUserValid()) return RedirectToLogin();

    try
    {
      var folderPath = Path.Combine(GetUserDirectory(), folderName);
      if (Directory.Exists(folderPath))
      {
        Directory.Delete(folderPath, true);

        return Json(new { success = true, message = "Pasta deletada com sucesso!" });
      }
      else
      {
        return Json(new { success = false, message = "Pasta não encontrada." });
      }
    }
    catch (Exception ex)
    {
      return Json(new { success = false, message = $"Erro ao deletar a pasta: {ex.Message}" });
    }
  }

  [HttpPost]
  public IActionResult DeleteFile(string fileUrl)
  {
    if (!IsUserValid()) return RedirectToLogin();

    try
    {
      fileUrl = fileUrl.Replace("/", "\\");
      var filePath = GetUserDirectory(true) + fileUrl;

      if (System.IO.File.Exists(filePath))
      {
        System.IO.File.Delete(filePath);
        return Json(new { success = true, message = "Arquivo deletado com sucesso!" });
      }
      else
      {
        return Json(new { success = false, message = "Arquivo não encontrado." });
      }
    }
    catch (Exception ex)
    {
      return Json(new { success = false, message = $"Erro ao deletar o arquivo: {ex.Message}" });
    }
  }

  public IActionResult RedirecToRelatorios()
  {
    return IsUserValid() ? RedirectToAction("Relatorios") : RedirectToLogin();
  }


  private bool IsUserValid() => _validateSession.IsUserValid();

  private IActionResult RedirectToLogin() => RedirectToAction("LoginBasic", "Auth");

  private IActionResult RedirectToErrorPage() => RedirectToAction("MiscError", "MiscError");

  private int GetUserIdFromSession() => HttpContext.Session.GetInt32("Id") ?? -1;

  private async Task<FuncionarioModel?> GetFuncionarioAsync(int id)
  {
    using var client = CreateHttpClient();
    var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");

    return response.IsSuccessStatusCode
        ? await response.Content.ReadFromJsonAsync<FuncionarioModel>()
        : null;
  }

  private HttpClient CreateHttpClient()
  {
    var handler = new HttpClientHandler
    {
      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };

    var client = new HttpClient(handler);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);
    return client;
  }

  private void SetFuncionarioDataInSession(FuncionarioModel funcionario)
  {
    _validateSession.SetFuncionarioId(funcionario.Id);
    _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());
  }

  private PartialModel CreatePartialModel(FuncionarioModel funcionario)
  {
    var frontModel = new FrontModel(GetUserIcon());
    var folders = GetFoldersFromDirectory();

    return new PartialModel
    {
      front = frontModel,
      funcionario = funcionario,
      Folders = folders
    };
  }

  private string GetUserIcon()
  {
    var patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
    var key = _configuration["Authentication:Secret"];

    ViewBag.Username = _validateSession.GetUsername();
    return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId(), _validateSession.GetGenero());
  }

  private void CreateFolder(string folderName)
  {
    var baseDirectory = GetUserDirectory();
    var folderPath = Path.Combine(baseDirectory, folderName);

    if (Directory.Exists(folderPath))
    {
      return;
    }

    Directory.CreateDirectory(folderPath);
  }

  private string GetUserDirectory() =>
      Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\Users\\{_validateSession.GetUserId()}\\Relatorios");

  private string GetUserDirectory(bool delete = true) =>
     Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot");

  private void SaveFile(string folderName, string fileName, IFormFile fileUpload)
  {

    var uploadExtension = Path.GetExtension(fileUpload.FileName);

    if (string.IsNullOrEmpty(uploadExtension))
    {
      throw new InvalidOperationException("O arquivo de upload não possui uma extensão.");
    }

    var fileNameWithCorrectExtension = Path.ChangeExtension(fileName, uploadExtension);

    var folderPath = Path.Combine(GetUserDirectory(), folderName);
    if (!Directory.Exists(folderPath))
    {
      throw new DirectoryNotFoundException();
    }

    var filePath = Path.Combine(folderPath, fileNameWithCorrectExtension);

    using var stream = new FileStream(filePath, FileMode.Create);

    fileUpload.CopyTo(stream);
  }

  private bool IsValidFile(IFormFile fileUpload) => fileUpload != null && fileUpload.Length > 0;

  public List<FolderViewModel> GetFoldersFromDirectory()
  {
    var directoryPath = GetUserDirectory();
    if (!Directory.Exists(directoryPath)) return new List<FolderViewModel>();

    return Directory.GetDirectories(directoryPath).Select(subdirectory =>
    {
      var folderName = Path.GetFileName(subdirectory);
      var files = Directory.GetFiles(subdirectory)
                           .OrderByDescending(f => System.IO.File.GetCreationTime(f))
                           .Select(f => GetFormattedPath(f, directoryPath)) // Ensure proper formatting
                           .ToList();

      return new FolderViewModel
      {
        Name = folderName,
        Files = files
      };
    }).ToList();

  }

  private string GetFormattedPath(string filePath, string baseDirectory)
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
}
