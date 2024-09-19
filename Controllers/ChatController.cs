using AspnetCoreMvcFull.Commands;
using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AspnetCoreMvcFull.Controllers
{
  public class ChatController : Controller
  {
    private readonly HttpClient _httpClient;
    private readonly IValidateSession _session;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(HttpClient httpClient, IValidateSession session, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor, IHubContext<ChatHub> hubContext)
    {
      _httpClient = httpClient;
      _session = session;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
      _hubContext = hubContext;
    }

    public async Task<IActionResult> Chat()
    {
      if (!_session.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      var usuarioAtualId = GetUsuarioAtualId();

      var funcionarios = await _httpClient.GetFromJsonAsync<List<FuncionarioModel>>("http://localhost:5235/api/Auth/funcionarios");
      var mensagens = await _httpClient.GetFromJsonAsync<List<MensagemModel>>("http://localhost:5235/api/Mensagem/mensagens");
      var funcionarioModel = funcionarios.FirstOrDefault(x => x.Id == usuarioAtualId);
      if (funcionarios.Any())
      {
        funcionarios.ForEach(func => func.ImagemUrl = UserIcon(func.Genero));
      }

      var frontModel = new FrontModel(UserIcon());
      var filteredFuncionarios = funcionarios
     .Where(x => x.Id != usuarioAtualId)
     .ToList();

      var viewModel = new PartialModel
      {
        UsuarioAtualId = usuarioAtualId,
        funcionario = funcionarioModel,
        Funcionarios = filteredFuncionarios,
        front = frontModel,
        Mensagens = mensagens.Where(m => m.RemetenteId == usuarioAtualId || m.DestinatarioId == usuarioAtualId).ToList()
      };

      return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ObterMensagens(Guid destinatarioId)
    {
      var mensagensModel = await _httpClient.GetFromJsonAsync<List<MensagemModel>>("http://localhost:5235/api/Mensagem/mensagens");
      if (mensagensModel != null)
      {
        var mensagens = mensagensModel.Where(m => (m.RemetenteId == _session.GetFuncionarioId() && m.DestinatarioId == destinatarioId) ||
                            (m.RemetenteId == destinatarioId && m.DestinatarioId == _session.GetFuncionarioId())).ToList();

        return Json(mensagens);
      }
      return Json(null);
    }

    [HttpPost]
    public async Task<IActionResult> EnviarMensagem(Guid destinatarioId, string conteudo)
    {
      var usuarioAtualId = GetUsuarioAtualId();

      var command = new CriarMensagemCommand(usuarioAtualId, destinatarioId, conteudo);
      var response = await _httpClient.PostAsJsonAsync("http://localhost:5235/api/Mensagem/criar", command);

      if (response.IsSuccessStatusCode)
      {
        var mensagemId = await response.Content.ReadFromJsonAsync<Guid>();
        return Json(new { success = true, mensagemId = mensagemId, usuarioAtualId = usuarioAtualId });
      }

      return BadRequest("Erro ao enviar mensagem");
    }

    [HttpPost]
    public async Task<IActionResult> AtualizarMensagem(Guid mensagemId, string novoConteudo)
    {
      var command = new AtualizarMensagemCommand(mensagemId, novoConteudo);
      var response = await _httpClient.PutAsJsonAsync("http://localhost:5235/api/Mensagem/atualizar", command);

      if (response.IsSuccessStatusCode)
      {
        return RedirectToAction("Index");
      }

      return BadRequest("Erro ao atualizar mensagem");
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoLida(Guid mensagemId)
    {
      var command = new MarcarMensagemComoLidaCommand(mensagemId);
      var response = await _httpClient.PutAsJsonAsync("http://localhost:5235/api/Mensagem/marcarComoLida", command);

      if (response.IsSuccessStatusCode)
      {
        return Json(new { success = true });
      }

      return Json(new { success = false });
    }

    [HttpPost]
    public async Task<IActionResult> DeletarMensagem(Guid mensagemId)
    {
      var command = new DeletarMensagemCommand(mensagemId);
      var response = await _httpClient.DeleteAsync($"http://localhost:5235/api/Mensagem/deletar/{mensagemId}");

      if (response.IsSuccessStatusCode)
      {
        return RedirectToAction("Index");
      }

      return BadRequest("Erro ao deletar mensagem");
    }

    private Guid GetUsuarioAtualId()
    {

      return _session.GetFuncionarioId();
    }
    private string UserIcon()
    {
      string patch = "";
      string key = "";
      patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      key = _configuration["Authentication:Secret"];

      ViewBag.Username = _session.GetUsername();
      return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, _session.GetUserId(), _session.GetGenero());
    }
    private string UserIcon(TipoGeneroEnum genero)
    {
      string patch = "";
      string key = "";
      patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      key = _configuration["Authentication:Secret"];

      return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, 0, genero);
    }
  }
}
