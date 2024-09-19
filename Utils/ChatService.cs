using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Utils
{
  public interface IChatService
  {
    Task<List<FuncionarioModel>> ObterFuncionariosComMensagensAsync(Guid funcionarioPrincipalId);
    Task<List<MensagemModel>> ObterMensagensAsync(Guid remetenteId, Guid destinatarioId);
    Task<bool> VerificarMensagensNaoLidasAsync(Guid remetenteId, Guid destinatarioId);
  }

  public class ChatService : IChatService
  {
    private readonly HttpClient _httpClient;

    public ChatService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<List<FuncionarioModel>> ObterFuncionariosComMensagensAsync(Guid funcionarioPrincipalId)
    {
      var response = await _httpClient.GetAsync($"api/mensagens/funcionarios/{funcionarioPrincipalId}");
      response.EnsureSuccessStatusCode();

      var funcionarios = await response.Content.ReadFromJsonAsync<List<FuncionarioModel>>();
      return funcionarios;
    }

    public async Task<List<MensagemModel>> ObterMensagensAsync(Guid remetenteId, Guid destinatarioId)
    {
      var response = await _httpClient.GetAsync($"api/mensagens/conversa?remetenteId={remetenteId}&destinatarioId={destinatarioId}");
      response.EnsureSuccessStatusCode();

      var mensagens = await response.Content.ReadFromJsonAsync<List<MensagemModel>>();
      return mensagens;
    }

    public async Task<bool> VerificarMensagensNaoLidasAsync(Guid remetenteId, Guid destinatarioId)
    {
      var response = await _httpClient.GetAsync($"api/mensagens/nao-lidas?remetenteId={remetenteId}&destinatarioId={destinatarioId}");
      response.EnsureSuccessStatusCode();

      var existeMensagemNaoLida = await response.Content.ReadFromJsonAsync<bool>();
      return existeMensagemNaoLida;
    }
  }

}
