using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils.Email;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTO;
using System.Net.Http;
using AspnetCoreMvcFull.Utils.Funcionarios;
using Newtonsoft.Json;
using System.Text;

namespace AspnetCoreMvcFull.Controllers
{
  public class FeriasAdminController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IRelatorioService _relatorioService;
    private readonly IEmailService _emailService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentController> _logger;
    private readonly IFuncionarioService _funcionarioService;

    public FeriasAdminController(IValidateSession validateSession, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor, IRelatorioService relatorioService, IEmailService emailService, IHttpClientFactory httpClientFactory, ILogger<DocumentController> logger, IFuncionarioService funcionarioService)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
      _relatorioService = relatorioService;
      _emailService = emailService;
      _httpClientFactory = httpClientFactory;
      _logger = logger;
      _funcionarioService  = funcionarioService;
    }


 
    public async Task<IActionResult> FeriasAdmin()
    {

      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      if (!_validateSession.HasAdministrator(_validateSession.GetPermissao()) || !_validateSession.HasAnalista(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

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

        var funcionarios = await _funcionarioService.ObterTodosFuncionariosAsync();
        if(funcionarios == null)
        {
          _logger.LogWarning("Funcionario data is null for Id: {Id}", id);
          return RedirectToAction("MiscError", "MiscError");
        }

        UpdateSessionWithFuncionario(funcionario);

        response = await client.GetAsync($"http://localhost:5235/api/ferias/ferias");

        var ferias = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<FeriasModel>>()
            : null;

        var partialModel = new PartialModel
        {
          front = new FrontModel(UserIcon()),
          funcionario = funcionario,
          Funcionarios = funcionarios
        };

        if (ferias != null)
        {
          partialModel.Ferias = ferias;
        }

        return View("FeriasAdmin", partialModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching Funcionario or Ferias data for Id: {Id}", id);
        return View("MiscError", "MiscError");
      }

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

    [Route("FeriasAdmin/AddVacation")]
    [HttpPost]
    public async Task<IActionResult> AddVacation([FromBody] FeriasModel model)
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      if (!_validateSession.HasAdministrator(_validateSession.GetPermissao()) || !_validateSession.HasAnalista(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

      if (ModelState.IsValid)
      {

        var response = await SendRegisterRequestAsync(model);

        var PDF = _relatorioService.GerarRelatorioFerias(model);
        _emailService.EnviarEmailFeriasProgramadas(model, "suporteffcomunicacoes@gmail.com", PDF);
        return Ok(new { id = model.Id });
      }
      return BadRequest();
    }
    private async Task<HttpResponseMessage> SendRegisterRequestAsync(FeriasModel model)
    {
      using var client = CreateHttpClient();
      var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
      return await client.PostAsync("http://localhost:5235/api/ferias/addferias", content);
    }
    private HttpClient CreateHttpClient()
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      };

      return new HttpClient(handler);
    }

    [Route("FeriasAdmin/UpdateVacation/{id}")]
    [HttpPut("{id}")]
    public IActionResult UpdateVacation(int id, [FromBody] FeriasModel model)
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      if (!_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

      if (model == null || id != model.Id)
      {
        return BadRequest();
      }

      //var existingVacation = feriasList.FirstOrDefault(f => f.Id == id);
      //if (existingVacation == null)
      //{
      //  return NotFound();
      //}

      //existingVacation.NomeFuncionario = model.NomeFuncionario;
      //existingVacation.DataInicio = model.DataInicio;
      //existingVacation.DataFim = model.DataFim;
      //existingVacation.Observacoes = model.Observacoes;

      return Json(new { success = true });
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteVacation(int id)
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      if (!_validateSession.HasAdministrator(_validateSession.GetPermissao()))
        return RedirectToAction("MiscError", "MiscError");

      //var model = feriasList.FirstOrDefault(f => f.Id == id);
      //if (model != null)
      //{
      //  feriasList.Remove(model);
      //  return NoContent(); // Success with no content to return
      //}
      return NotFound();
    }
  }
}
