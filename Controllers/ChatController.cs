using AspnetCoreMvcFull.Commands;
using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.Utils.Funcionarios;
using AspnetCoreMvcFull.Utils.Messagem;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AspnetCoreMvcFull.Controllers
{
  public class ChatController : Controller
  {
    private readonly IMessageService _messageService;
    private readonly IFuncionarioService _funcionarioService;
    private readonly IValidateSession _session;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IMessageService messageService, IFuncionarioService funcionarioService, IValidateSession session, IFileEncryptor fileEncryptor, IHubContext<ChatHub> hubContext)
    {
      _messageService = messageService;
      _funcionarioService = funcionarioService;
      _session = session;
      _fileEncryptor = fileEncryptor;
      _hubContext = hubContext;
    }

    public async Task<IActionResult> Chat()
    {
      if (!_session.IsUserValid())
      {
        return RedirectToAction("LoginBasic", "Auth");
      }

      var usuarioAtualId = _session.GetFuncionarioId();
      var funcionarios = await _funcionarioService.ObterTodosFuncionariosAsync();
      var mensagens = await _messageService.ObterMensagensAsync();

      if (funcionarios.Any())
      {
        funcionarios.ForEach(func => func.ImagemUrl = UserIcon(func.Genero));
      }

      var funcionarioAtual = funcionarios.FirstOrDefault(x => x.Id == usuarioAtualId);
      var viewModel = CriarPartialModel(usuarioAtualId, funcionarioAtual, funcionarios, mensagens);

      return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ObterMensagens(Guid destinatarioId)
    {
      var mensagens = await _messageService.ObterMensagensAsync();
      var filteredMensagens = mensagens.Where(m => (m.RemetenteId == _session.GetFuncionarioId() && m.DestinatarioId == destinatarioId) ||
                                                   (m.RemetenteId == destinatarioId && m.DestinatarioId == _session.GetFuncionarioId()))
                                       .ToList();

      return Json(filteredMensagens);
    }

    [HttpPost]
    public async Task<IActionResult> EnviarMensagem(Guid destinatarioId, string conteudo)
    {
      var usuarioAtualId = GetUsuarioAtualId();
      var sanitizer = new HtmlSanitizer();
      conteudo = sanitizer.Sanitize(conteudo);

      var command = new CriarMensagemCommand(usuarioAtualId, destinatarioId, conteudo);
      var mensagemId = await _messageService.EnviarMensagemAsync(command);

      if (mensagemId.HasValue)
      {
        return Json(new { success = true, mensagemId = mensagemId.Value, usuarioAtualId = usuarioAtualId });
      }

      return BadRequest("Erro ao enviar mensagem");
    }

    [HttpPost]
    public async Task<IActionResult> AtualizarMensagem(Guid mensagemId, string novoConteudo)
    {
      var command = new AtualizarMensagemCommand(mensagemId, novoConteudo);
      var success = await _messageService.AtualizarMensagemAsync(command);

      if (success)
      {
        return RedirectToAction("Index");
      }

      return BadRequest("Erro ao atualizar mensagem");
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoLida(Guid mensagemId)
    {
      var command = new MarcarMensagemComoLidaCommand(mensagemId);
      var success = await _messageService.MarcarComoLidaAsync(command);

      return Json(new { success = success });
    }

    [HttpPost]
    public async Task<IActionResult> DeletarMensagem(Guid mensagemId)
    {
      var success = await _messageService.DeletarMensagemAsync(mensagemId);

      if (success)
      {
        return RedirectToAction("Index");
      }

      return BadRequest("Erro ao deletar mensagem");
    }

    private PartialModel CriarPartialModel(Guid usuarioAtualId, FuncionarioModel funcionario, List<FuncionarioModel> funcionarios, List<MensagemModel> mensagens)
    {
      var filteredFuncionarios = funcionarios.Where(x => x.Id != usuarioAtualId).ToList();

      return new PartialModel
      {
        UsuarioAtualId = usuarioAtualId,
        funcionario = funcionario,
        Funcionarios = filteredFuncionarios,
        front = new FrontModel(UserIcon(_session.GetGenero())),
        Mensagens = mensagens.Where(m => m.RemetenteId == usuarioAtualId || m.DestinatarioId == usuarioAtualId).ToList()
      };
    }

    private string SanitizeInput(string input)
    {
      var sanitizer = new HtmlSanitizer();
      return sanitizer.Sanitize(input);
    }

    private string UserIcon(TipoGeneroEnum genero = default)
    {
      string patch = Path.Combine(_fileEncryptor.GetRootPath(), "Users");
      string key = _fileEncryptor.GetSecretKey();
      return _fileEncryptor.UserIconUrl(patch, key, _session.GetUserId().ToString(),0, genero);
    }

    private Guid GetUsuarioAtualId()
    {
      return _session.GetFuncionarioId();
    }
  }
}
