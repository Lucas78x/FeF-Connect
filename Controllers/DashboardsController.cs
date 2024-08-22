using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Utils.PDF;
using System.Text;
using AspnetCoreMvcFull.Enums;


namespace AspnetCoreMvcFull.Controllers;

public class DashboardsController : Controller
{
  private readonly IValidateSession _validateSession;
  private readonly IConfiguration _configuration;
  private readonly IWebHostEnvironment _webHostEnvironment;
  private readonly IFileEncryptor _fileEncryptor;
  private readonly IRelatorioService _relatorioService;

  public DashboardsController(IValidateSession validateSession, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor, IRelatorioService relatorioService)
  {
    _validateSession = validateSession;
    _configuration = configuration;
    _webHostEnvironment = webHostEnvironment;
    _fileEncryptor = fileEncryptor;
    _relatorioService = relatorioService;  
  }

  public async Task<IActionResult> Dashboard()
  {
    if (!_validateSession.IsUserValid())
    {
      return RedirectToAction("LoginBasic", "Auth");
    }

    var id = HttpContext.Session.GetInt32("Id") ?? -1;
    if (id == -1)
    {
      return RedirectToAction("LoginBasic", "Auth");
    }

    try
    {
      using var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      using var client = new HttpClient(handler);
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

      var funcionarioResponse = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");
      if (!funcionarioResponse.IsSuccessStatusCode)
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      var funcionario = await funcionarioResponse.Content.ReadFromJsonAsync<FuncionarioDTO>();
      if (funcionario == null)
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);
      _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());

      var frontModel = new FrontModel(UserIcon());
      var partialModel = new PartialModel
      {
        front = frontModel,
        funcionario = funcionario
      };

      var requisicoesResponse = await client.GetAsync($"http://localhost:5235/api/Auth/listaRequisicoes");
      if (requisicoesResponse.IsSuccessStatusCode)
      {
        var requisicoes = await requisicoesResponse.Content.ReadFromJsonAsync<List<RequisicoesModel>>();
        if (requisicoes != null)
        {
          var requisicoesPorStatus = requisicoes
              .GroupBy(r => r.Status)
              .Select(group => new
              {
                Status = group.Key.ToString(),
                Count = group.Count()
              }).ToList();

          ViewBag.Statuses = requisicoesPorStatus.Select(r => r.Status).ToList();
          ViewBag.Counts = requisicoesPorStatus.Select(r => r.Count).ToList();
        }
        else
        {
          ViewBag.Counts = 0;
        }
      }
      else
      {
        ViewBag.Counts = 0;
      }

      return View(partialModel);
    }
    catch
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
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
  private string UserCheque()
  {
    string patch = "";
    string key = "";
    patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
    key = _configuration["Authentication:Secret"];

    ViewBag.Username = _validateSession.GetUsername();
    return _fileEncryptor.UserContraCheque(patch, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId());
  }
  private string UserEscala()
  {
    string patch = "";
    string key = "";
    patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
    key = _configuration["Authentication:Secret"];

    ViewBag.Username = _validateSession.GetUsername();
    return _fileEncryptor.UserEscala(patch, _webHostEnvironment.WebRootPath, key);
  }


  //TODO: Separar Depois
  public async Task<IActionResult> Pessoal()
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

      var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioDTO>();
      if (funcionario == null)
      {
        return RedirectToPage("MiscError", "Pages");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);

      var frontModel = new FrontModel(UserIcon());
      var partialModel = new PartialModel
      {
        front = frontModel,
        funcionario = funcionario
      };

      return View("Pessoal", partialModel);
    }
    catch
    {
      return View("MiscError", "Pages");
    }
  }
  public IActionResult RedirecToPerson()
  {
    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("Pessoal", "Dashboards");
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }

  public async Task<IActionResult> ChangePhoto(IFormFile file)
  {
    if (_validateSession.IsUserValid())
    {
      await ChangePhotoAsync(file);
      return Json(new { success = true });
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }

  private async Task ChangePhotoAsync(IFormFile photoUpload)
  {
    string patch = "";
    string key = "";
    patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
    key = _configuration["Authentication:Secret"];

    await _fileEncryptor.ChangePhoto(photoUpload, patch, key, _validateSession.GetUserId());
  }

  public async Task<IActionResult> Settings()
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

      var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioDTO>();
      if (funcionario == null)
      {
        return RedirectToPage("MiscError", "Pages");
      }

      _validateSession.SetFuncionarioId(funcionario.Id);

      var frontModel = new FrontModel(UserIcon());
      var partialModel = new PartialModel
      {
        front = frontModel,
        funcionario = funcionario
      };

      return View("Settings", partialModel);
    }
    catch
    {
      return RedirectToPage("MiscError", "Pages");
    }
  }
  public IActionResult RedirecToSettings()
  {
    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("Settings", "Dashboards");
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }
  public async Task<IActionResult> ContraCheque()
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
            var frontModel = new FrontModel(UserIcon());
            var contraModel = new FrontModel(UserCheque());
            var partialModel = new PartialModel();
            partialModel.front = frontModel;
            partialModel.funcionario = funcionario;
            partialModel.UtilUrl = contraModel;

            return View("ContraCheque", partialModel);
          }
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
    return RedirectToAction("LoginBasic", "Auth");
  }
  public IActionResult RedirecToCheque()
  {
    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("ContraCheque", "Dashboards");
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }
  public async Task<IActionResult> Escala()
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
            var frontModel = new FrontModel(UserIcon());
            var escalaModel = new FrontModel(UserEscala());
            var partialModel = new PartialModel();
            partialModel.front = frontModel;
            partialModel.funcionario = funcionario;
            partialModel.UtilUrl = escalaModel;

            return View("Escala", partialModel);
          }
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
    return RedirectToAction("LoginBasic", "Auth");
  }
  public IActionResult RedirecToEscala()
  {
    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("Escala", "Dashboards");
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }
  public async Task<IActionResult> Requisicoes()
  {
    if (_validateSession.IsUserValid())
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      var client = new HttpClient(handler);

      try
      {

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

        var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={_validateSession.GetUserId()}");
        if (response.IsSuccessStatusCode)
        {
          var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioDTO>();
          if (funcionario != null)
          {
            _validateSession.SetFuncionarioId(funcionario.Id);
            var frontModel = new FrontModel(UserIcon());
            var escalaModel = new FrontModel(UserEscala());
            var partialModel = new PartialModel();
            partialModel.front = frontModel;
            partialModel.funcionario = funcionario;
            partialModel.UtilUrl = escalaModel;

            response = await client.PostAsJsonAsync("http://localhost:5235/api/Auth/requisicoes", funcionario);

            if (response.IsSuccessStatusCode)
            {
              var requisicoes = await response.Content.ReadFromJsonAsync<List<RequisicoesModel>>();
              if (requisicoes != null)
              {
                partialModel.Requisicoes = requisicoes;
              }
            }

            return View("Requisicoes", partialModel);
          }
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
    return RedirectToAction("LoginBasic", "Auth");
  }
  public IActionResult RedirecToRequisicoes()
  {
    if (_validateSession.IsUserValid())
    {
      return RedirectToAction("Requisicoes", "Dashboards");
    }
    else
    {
      return RedirectToAction("LoginBasic", "Auth");
    }
  }

  [HttpPost]
  public async Task<IActionResult> GerarPDF(Guid id)
  {
    var handler = new HttpClientHandler
    {
      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };

    var client = new HttpClient(handler);

    try
    {

      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

      var response = await client.GetAsync($"http://localhost:5235/api/Auth/requisicao?Id={id}");
      if (response.IsSuccessStatusCode)
      {
        var requisicao = await response.Content.ReadFromJsonAsync<RequisicoesModel>();
        if (requisicao != null)
        {
          var pdfBytes = _relatorioService.GerarRelatorio(requisicao);

          return File(pdfBytes, "application/pdf", "requisicao-" + id + ".pdf");
        }
      }
    }
    catch
    {
      return BadRequest();
    }

    return BadRequest();
  }

  [HttpPost]
  public async Task<IActionResult> Atualizar([FromBody] RequisicaoModel model)
  {
    if (ModelState.IsValid)
    {

      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      var client = new HttpClient(handler);

      var requestBody = new
      {
        Id = model.Id,
        Requisitante = model.Requisitante,
        Descricao = model.Descricao,
        DescricaoSolucao = model.DescricaoSolucao,
        Status = model.Status,
        DataFinalizacao = model.DataFinalizacao
      };
      var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

      try
      {

        var response = await client.PostAsync("http://localhost:5235/api/Auth/atualizarRequisicao", content);

        if (response.IsSuccessStatusCode)
        {
          return Json(new { success = true });
        }
        else
        {
          return Json(new { success = false, message = "Requisição não encontrada." });

        }
      }
      catch
      {
        return Json(new { success = false, message = "Ocorreu um erro inesperado." });

      }
    }

    return Json(new { success = false, message = "Dados inválidos." });
  }

  [HttpGet]
  public async Task<IActionResult> FiltrarRequisicoes(string requisitante, string status)
  {

    var handler = new HttpClientHandler
    {
      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };

    var client = new HttpClient(handler);

    try
    {

      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

      var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={_validateSession.GetUserId()}");
      if (response.IsSuccessStatusCode)
      {
        var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioDTO>();
        if (funcionario != null)
        {

          response = await client.PostAsJsonAsync("http://localhost:5235/api/Auth/requisicoes", funcionario);

          if (response.IsSuccessStatusCode)
          {
            var requisicoes = await response.Content.ReadFromJsonAsync<List<RequisicoesModel>>();
            if (requisicoes != null)
            {
              List<RequisicoesModel> filtroRequisicoes = null;

              if (!string.IsNullOrEmpty(requisitante) && !string.IsNullOrEmpty(status))
              {
                filtroRequisicoes = requisicoes.Where(x => x.Status == (TipoStatusEnum)Enum.Parse(typeof(TipoStatusEnum), status) && x.RequisitanteNome.ToUpper() == requisitante.ToUpper()).ToList();
              }
              else if (!string.IsNullOrEmpty(status))
              {
                filtroRequisicoes = requisicoes.Where(x => x.Status == (TipoStatusEnum)Enum.Parse(typeof(TipoStatusEnum), status)).ToList();
              }
              else if (!string.IsNullOrEmpty(requisitante))
              {
                filtroRequisicoes = requisicoes.Where(x => x.RequisitanteNome.ToUpper() == requisitante.ToUpper()).ToList();
              }
              else
              {
                filtroRequisicoes = requisicoes;
              }

              return Json(new
              {
                success = true,
                requisicoes = filtroRequisicoes
              });

            }
          }

        }
      }

      return Json(new
      {
        success = false,
        message = "Erro inesperado"
      });
    }
    catch (Exception ex)
    {
      return Json(new
      {
        success = false,
        message = ex.Message
      });

    }
  }

}



