using AspnetCoreMvcFull.Commands;
using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Utils.Messagem
{
  public class MessageService : IMessageService
  {
    private readonly HttpClient _httpClient;

    public MessageService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<List<MensagemModel>> ObterMensagensAsync()
    {
      return await _httpClient.GetFromJsonAsync<List<MensagemModel>>("http://localhost:5235/api/Mensagem/mensagens");
    }

    public async Task<Guid?> EnviarMensagemAsync(CriarMensagemCommand command)
    {
      var response = await _httpClient.PostAsJsonAsync("http://localhost:5235/api/Mensagem/criar", command);

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<Guid>();
      }

      return null;
    }

    public async Task<bool> AtualizarMensagemAsync(AtualizarMensagemCommand command)
    {
      var response = await _httpClient.PutAsJsonAsync("http://localhost:5235/api/Mensagem/atualizar", command);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarcarComoLidaAsync(MarcarMensagemComoLidaCommand command)
    {
      var response = await _httpClient.PutAsJsonAsync("http://localhost:5235/api/Mensagem/marcarComoLida", command);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletarMensagemAsync(Guid mensagemId)
    {
      var response = await _httpClient.DeleteAsync($"http://localhost:5235/api/Mensagem/deletar/{mensagemId}");
      return response.IsSuccessStatusCode;
    }
  }

}
