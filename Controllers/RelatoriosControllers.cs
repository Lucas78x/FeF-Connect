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

  public RelatoriosController(IValidateSession validateSession, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor, IRelatorioService relatorioService)
  {
    _validateSession = validateSession;
    _configuration = configuration;
    _webHostEnvironment = webHostEnvironment;
    _fileEncryptor = fileEncryptor;
    _relatorioService = relatorioService;
  }

  public async Task<IActionResult> Relatorios()
  {
    if (!_validateSession.IsUserValid())
    {
      return RedirectToAction("LoginBasic", "Auth");
    }

    var id = HttpContext.Session.GetInt32("Id") ?? -1;
    if (id == -1)
    {
      return RedirectToPage("MiscError", "Pages");
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
        return RedirectToPage("MiscError", "Pages");
      }

      var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioModel>();
      if (funcionario == null)
      {
        return RedirectToPage("MiscError", "Pages");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);
      _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());

      var frontModel = new FrontModel(UserIcon());
      var partialModel = new PartialModel
      {
        front = frontModel,
        funcionario = funcionario,
        Folders= GetFoldersFromDirectory()
      };

      return View("Relatorios", partialModel);
    }
    catch
    {
      return View("MiscError", "Pages");
    }
  }

  public List<FolderViewModel> GetFoldersFromDirectory()
  {
    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/Users/{_validateSession.GetUserId()}/Relatorios");

    if (!Directory.Exists(directoryPath))
    {
      return new List<FolderViewModel>();
    }

    var folders = new List<FolderViewModel>();

    // Obter todas as subpastas (se houver)
    var subdirectories = Directory.GetDirectories(directoryPath);

    foreach (var subdirectory in subdirectories)
    {
      var folderName = Path.GetFileName(subdirectory);
      var folderViewModel = new FolderViewModel
      {
        Name = folderName,
        Files = new List<string>() // Inicialize a lista de arquivos
      };

      var files = Directory.GetFiles(subdirectory)
                           .OrderByDescending(file => System.IO.File.GetCreationTime(file))
                           .ToArray();

      foreach (var file in files)
      {
        var fileName = Path.GetFileName(file);
        folderViewModel.Files.Add(fileName);
      }

      folders.Add(folderViewModel);
    }

    return folders;
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
  public IActionResult AddFolder(string folderName)
  {
    // Definir o caminho base onde as pastas são armazenadas
    var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/Users/{_validateSession.GetUserId()}/Relatorios");

    // Criar o caminho completo para a nova pasta
    var newFolderPath = Path.Combine(baseDirectory, folderName);

    // Verificar se a pasta já existe
    if (!Directory.Exists(newFolderPath))
    {
      // Criar a nova pasta
      Directory.CreateDirectory(newFolderPath);
    }
    else
    {
      // Se a pasta já existe, você pode retornar um erro ou mensagem de aviso
      TempData["ErrorMessage"] = "A pasta já existe.";
    }

    // Redirecionar para a ação que lista as pastas, passando uma mensagem se houver erro
    return RedirectToAction("Relatorios");
  }

  [HttpPost]
  public IActionResult AddFile(string folderName, string fileName, IFormFile fileUpload)
  {
    if (fileUpload == null || fileUpload.Length == 0)
    {
      return Json(new { success = false });
    }

    // Definir o caminho base onde as pastas e arquivos são armazenados
    var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/Users/{_validateSession.GetUserId()}/Relatorios");

    // Criar o caminho completo da pasta e do arquivo
    var folderPath = Path.Combine(baseDirectory, folderName);

    // Verificar se a pasta existe
    if (!Directory.Exists(folderPath))
    {

      return Json(new { success = false });
    }

    // Criar o caminho completo para o novo arquivo
    var filePath = Path.Combine(folderPath, fileName);

    try
    {
      // Salvar o arquivo no diretório especificado
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        fileUpload.CopyTo(stream);
      }
    }
    catch (Exception ex)
    {
      return Json(new { success = false });
    }

    return Json(new { success = true });
  }

  public IActionResult RedirecToRelatorios()
  {
    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("Relatorios", "Relatorios");
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }
}

