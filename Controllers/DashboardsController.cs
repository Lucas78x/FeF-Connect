using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils.Email;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Enums;
using System.Text;

namespace AspnetCoreMvcFull.Controllers
{
  public class DashboardsController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IRelatorioService _relatorioService;
    private readonly IEmailService _emailService;
    private readonly HttpClient _httpClient;

    public DashboardsController(IValidateSession validateSession, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor, IRelatorioService relatorioService, IEmailService emailService, HttpClient client)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
      _relatorioService = relatorioService;
      _emailService = emailService;
      _httpClient = client;
    }


    public async Task<IActionResult> Dashboard()
    {
      if (!IsUserValid()) return RedirectToLogin();

      var userId = GetUserId();
      if (userId == -1) return RedirectToLogin();

      try
      {
        var funcionario = await FetchFuncionarioById(userId);
        if (funcionario == null) return RedirectToLogin();

        PrepareSession(funcionario);
        await SetViewBagRequisicoesAsync();

        var partialModel = CreatePartialModel(funcionario);
        return View(partialModel);
      }
      catch
      {
        return RedirectToLogin();
      }
    }

    private async Task<FuncionarioModel> FetchFuncionarioById(int id)
    {
      using var client = CreateHttpClient();
      var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");
      return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<FuncionarioModel>() : null;
    }

    private async Task SetViewBagRequisicoesAsync()
    {
      using var client = CreateHttpClient();
      var response = await client.GetAsync($"http://localhost:5235/api/Auth/listaRequisicoes");

      if (!response.IsSuccessStatusCode)
      {
        ViewBag.Counts = 0;
        return;
      }

      var requisicoes = await response.Content.ReadFromJsonAsync<List<RequisicoesModel>>();
      if (requisicoes == null)
      {
        ViewBag.Counts = 0;
        return;
      }

      var requisicoesPorStatus = requisicoes
          .GroupBy(r => r.Status)
          .Select(group => new { Status = group.Key.ToString(), Count = group.Count() })
          .ToList();

      ViewBag.Statuses = requisicoesPorStatus.Select(r => r.Status).ToList();
      ViewBag.Counts = requisicoesPorStatus.Select(r => r.Count).ToList();
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

    private PartialModel CreatePartialModel(FuncionarioModel funcionario)
    {
      var frontModel = new FrontModel(GetUserIcon());
      return new PartialModel
      {
        front = frontModel,
        funcionario = funcionario
      };
    }

    private string GetUserIcon()
    {
      string path = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      string key = _configuration["Authentication:Secret"];

      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserIconUrl(path, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId(), _validateSession.GetGenero());
    }

    private bool IsUserValid() => _validateSession.IsUserValid();

    private int GetUserId() => HttpContext.Session.GetInt32("Id") ?? -1;

    private void PrepareSession(FuncionarioModel funcionario)
    {
      _validateSession.SetFuncionarioId(funcionario.Id);
      _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());
    }

    private IActionResult RedirectToLogin() => RedirectToAction("LoginBasic", "Auth");


    public async Task<IActionResult> Pessoal()
    {
      if (!IsUserValid()) return RedirectToLogin();

      var userId = GetUserId();
      if (userId == -1) return RedirectToErrorPage();

      try
      {
        var funcionario = await FetchFuncionarioById(userId);
        if (funcionario == null) return RedirectToErrorPage();

        _validateSession.SetFuncionarioId(funcionario.Id);
        var partialModel = CreatePartialModel(funcionario);
        return View("Pessoal", partialModel);
      }
      catch
      {
        return RedirectToErrorPage();
      }
    }

    private IActionResult RedirectToErrorPage() => RedirectToAction("MiscError", "MiscError");


    public async Task<IActionResult> ChangePhoto(IFormFile file)
    {
      if (!IsUserValid()) return RedirectToLogin();

      await ChangePhotoAsync(file);
      return Json(new { success = true });
    }

    private async Task ChangePhotoAsync(IFormFile photoUpload)
    {
      string path = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      string key = _configuration["Authentication:Secret"];

      await _fileEncryptor.ChangePhoto(photoUpload, path, key, _validateSession.GetUserId());
    }

    public IActionResult RedirecToPerson()
    {
      return IsUserValid() ? RedirectToAction("Pessoal", "Dashboards") : RedirectToLogin();
    }

    private string GetUserPatch()
    {
      return Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
    }

    private string GetUserKey()
    {
      return _configuration["Authentication:Secret"];
    }

    private List<PayslipModel> UserCheque()
    {
      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserCheque(GetUserPatch(), _webHostEnvironment.WebRootPath, GetUserKey(), _validateSession.GetUserId());
    }

    private string UserEscala()
    {
      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserEscala(GetUserPatch(), _webHostEnvironment.WebRootPath, GetUserKey());
    }

    public async Task<IActionResult> ContraCheque()
    {
      if (!_validateSession.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      var userId = HttpContext.Session.GetInt32("Id") ?? -1;
      var funcionario = await FetchFuncionarioById(userId);

      if (funcionario == null)
      {
        return RedirectToAction("MiscError", "MiscError");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);

      var partialModel = new PartialModel
      {
        funcionario = funcionario,
        front = new FrontModel(GetUserIcon()),
        Payslips = UserCheque(),
        FilterDate = DateTime.Now,
      };

      return View("ContraCheque", partialModel);
    }

    public IActionResult RedirecToCheque()
    {
      return _validateSession.IsUserValid()
          ? RedirectToAction("ContraCheque", "Dashboards")
          : RedirectToAction("LoginBasic", "Auth");
    }

    public async Task<IActionResult> Escala()
    {
      if (!_validateSession.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      var id = HttpContext.Session.GetInt32("Id") ?? -1;
      var funcionario = await FetchFuncionarioById(id);

      if (funcionario == null)
      {
        return RedirectToAction("MiscError", "MiscError");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);

      var partialModel = new PartialModel
      {
        front = new FrontModel(GetUserIcon()),
        funcionario = funcionario,
        UtilUrl = new FrontModel(UserEscala())
      };

      return View("Escala", partialModel);
    }

    public IActionResult RedirecToEscala()
    {
      return _validateSession.IsUserValid()
          ? RedirectToAction("Escala", "Dashboards")
          : RedirectToAction("LoginBasic", "Auth");
    }

    public async Task<IActionResult> Requisicoes()
    {
      if (!_validateSession.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      var userId = _validateSession.GetUserId();
      var funcionario = await FetchFuncionarioById(userId);

      if (funcionario == null)
      {
        return RedirectToAction("MiscError", "MiscError");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);

      var frontModel = new FrontModel(GetUserIcon());
      var escalaModel = new FrontModel(UserEscala());
      var partialModel = new PartialModel
      {
        front = frontModel,
        funcionario = funcionario,
        UtilUrl = escalaModel
      };

      var requisicoesResponse = await FetchRequisicoesAsync(funcionario);
      if (requisicoesResponse != null)
      {
        partialModel.Requisicoes = requisicoesResponse;
      }

      var funcionarios = await _httpClient.GetFromJsonAsync<List<FuncionarioModel>>("http://localhost:5235/api/Auth/funcionarios");
      if (funcionarios != null)
      {
        partialModel.Funcionarios = funcionarios.Where(x => x.Id != funcionario.Id).ToList();
      }

      return View("Requisicoes", partialModel);
    }

    private async Task<List<RequisicoesModel>> FetchRequisicoesAsync(FuncionarioModel funcionario)
    {
      using var client = CreateHttpClient();
      var response = await client.PostAsJsonAsync("http://localhost:5235/api/Auth/requisicoes", funcionario);
      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<List<RequisicoesModel>>();
      }
      return null;
    }

    public IActionResult RedirecToRequisicoes()
    {
      return _validateSession.IsUserValid()
          ? RedirectToAction("Requisicoes", "Dashboards")
          : RedirectToAction("LoginBasic", "Auth");
    }


    [HttpPost]
    public async Task<IActionResult> GerarPDF(Guid id)
    {
      try
      {
        var requisicao = await GetRequisicaoByIdAsync(id);
        if (requisicao != null)
        {
          var pdfBytes = _relatorioService.GerarRelatorio(requisicao);
          return File(pdfBytes, "application/pdf", $"requisicao-{id}.pdf");
        }
      }
      catch
      {
        return BadRequest("Failed to generate PDF.");
      }

      return BadRequest("Requisição não encontrada.");
    }

    [HttpPost]
    public async Task<IActionResult> Atualizar([FromBody] RequisicaoModel model)
    {
      if (!ModelState.IsValid)
        return Json(new { success = false, message = "Dados inválidos." });

      try
      {
        var requisicaoModel = await UpdateRequisicaoAsync(model);
        if (requisicaoModel != null)
        {
          await SendEmails(requisicaoModel, model);
          return Json(new { success = true });
        }
      }
      catch
      {
        return Json(new { success = false, message = "Ocorreu um erro inesperado." });
      }

      return Json(new { success = false, message = "Requisição não encontrada." });
    }

    [HttpGet]
    public async Task<IActionResult> FiltrarRequisicoes(string requisitante, string status)
    {
      try
      {
        var funcionario = await FetchFuncionarioById(_validateSession.GetUserId());
        if (funcionario != null)
        {
          var requisicoes = await GetRequisicoesAsync(funcionario);
          var filtroRequisicoes = FilterRequisicoes(requisicoes, requisitante, status);
          return Json(new { success = true, requisicoes = filtroRequisicoes });
        }
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }

      return Json(new { success = false, message = "Erro inesperado" });
    }

    private async Task<RequisicoesModel> GetRequisicaoByIdAsync(Guid id)
    {
      using var client = CreateHttpClient();
      var response = await client.GetAsync($"http://localhost:5235/api/Auth/requisicao?Id={id}");
      return response.IsSuccessStatusCode
          ? await response.Content.ReadFromJsonAsync<RequisicoesModel>()
          : null;
    }

    private async Task<RequisicoesModel> UpdateRequisicaoAsync(RequisicaoModel model)
    {
      using var client = CreateHttpClient();
      var requestBody = new
      {
        model.Id,
        model.Requisitante,
        model.Descricao,
        model.DescricaoSolucao,
        model.Status,
        model.DataFinalizacao
      };

      var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
      var response = await client.PostAsync("http://localhost:5235/api/Auth/atualizarRequisicao", content);

      return response.IsSuccessStatusCode
          ? await response.Content.ReadFromJsonAsync<RequisicoesModel>()
          : null;
    }



    private async Task<List<RequisicoesModel>> GetRequisicoesAsync(FuncionarioModel funcionario)
    {
      using var client = CreateHttpClient();
      var response = await client.PostAsJsonAsync("http://localhost:5235/api/Auth/requisicoes", funcionario);
      return response.IsSuccessStatusCode
          ? await response.Content.ReadFromJsonAsync<List<RequisicoesModel>>()
          : new List<RequisicoesModel>();
    }

    private async Task SendEmails(RequisicoesModel requisicaoModel, RequisicaoModel model)
    {
      var funcionarios = await GetFuncionariosAsync();
      var requisitante = funcionarios.FirstOrDefault(x => x.Id == requisicaoModel.RequisitanteId);
      var destinatario = funcionarios.FirstOrDefault(x => x.Id == requisicaoModel.FuncionarioId);

      if (requisitante != null)
      {
        var pdfBytes = _relatorioService.GerarRelatorio(requisicaoModel);
        _emailService.EnviarEmail(
            $"{requisitante.Nome} {requisitante.Sobrenome}",
            model.Descricao,
            requisitante.Email,
            requisicaoModel.DescricaoSolucao,
            requisicaoModel.Status,
            pdfBytes
        );
      }

      if (destinatario != null)
      {
        var pdfBytes = _relatorioService.GerarRelatorio(requisicaoModel);
        _emailService.EnviarEmail(
            $"{destinatario.Nome} {destinatario.Sobrenome}",
            model.Descricao,
            destinatario.Email,
            requisicaoModel.DescricaoSolucao,
            requisicaoModel.Status,
            pdfBytes
        );
      }
      else
      {
        if (requisicaoModel != null)
        {
          var destionation = funcionarios.Where(x => x.Permissao == requisicaoModel.Setor && x.Email != requisitante?.Email).ToList();
          if (destionation.Any())
          {
            var pdfBytes = _relatorioService.GerarRelatorio(requisicaoModel);

            foreach (var func in destionation)
            {

              _emailService.EnviarEmail(
                  $"{func.Nome} {func.Sobrenome}",
                  model.Descricao,
                  func.Email,
                  requisicaoModel.DescricaoSolucao,
                  requisicaoModel.Status,
                  pdfBytes
              );
            }
          }
        }
      }
    }

    private async Task<List<FuncionarioModel>> GetFuncionariosAsync()
    {
      using var client = CreateHttpClient();
      var response = await client.GetAsync("http://localhost:5235/api/Auth/funcionarios");
      return response.IsSuccessStatusCode
          ? await response.Content.ReadFromJsonAsync<List<FuncionarioModel>>()
          : new List<FuncionarioModel>();
    }

    private List<RequisicoesModel> FilterRequisicoes(List<RequisicoesModel> requisicoes, string requisitante, string status)
    {
      return requisicoes.Where(x =>
          (string.IsNullOrEmpty(requisitante) || x.RequisitanteNome.ToUpper() == requisitante.ToUpper()) &&
          (string.IsNullOrEmpty(status) || x.Status == (TipoStatusEnum)Enum.Parse(typeof(TipoStatusEnum), status))
      ).ToList();
    }


  }
}
