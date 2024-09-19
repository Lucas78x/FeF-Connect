using AspnetCoreMvcFull.DTO;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;

public class ChatHub : Hub
{
  private readonly HttpClient _httpClient;
  public ChatHub(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }
  public async Task SendMessage(Guid destinatarioId, string conteudo, string mensagemId, string usuarioAtualId, string remetenteNome)
  {

    await Clients.User(destinatarioId.ToString()).SendAsync("ReceiveMessage", usuarioAtualId,destinatarioId.ToString(), conteudo, remetenteNome, mensagemId);
    await Clients.Caller.SendAsync("ReceiveMessage", usuarioAtualId, destinatarioId.ToString(), conteudo, remetenteNome,mensagemId);
  }
  public async Task MessageRead(string mensagemId)
  {

    var mensagem = await _httpClient.GetFromJsonAsync<MensagemModel>($"http://localhost:5235/api/Mensagem/mensagem/{mensagemId}");

    if (mensagem != null)
    {
      string remetenteId = mensagem.RemetenteId.ToString();
      string destinatarioId = mensagem.DestinatarioId.ToString();

      // Notificar o remetente que a mensagem foi lida
      await Clients.User(remetenteId).SendAsync("MessageReadNotification", mensagemId, destinatarioId);
    }
  }
}


