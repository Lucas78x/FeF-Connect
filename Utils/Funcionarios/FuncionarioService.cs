using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Utils.Funcionarios
{
  public class FuncionarioService : IFuncionarioService
  {
    private readonly HttpClient _httpClient;

    public FuncionarioService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<FuncionarioModel> ObterFuncionarioPorIdAsync(Guid funcionarioId)
    {
      return await _httpClient.GetFromJsonAsync<FuncionarioModel>($"http://localhost:5235/api/Auth/Funcionario/{funcionarioId}");
    }

    public async Task<List<FuncionarioModel>> ObterTodosFuncionariosAsync()
    {
      return await _httpClient.GetFromJsonAsync<List<FuncionarioModel>>("http://localhost:5235/api/Auth/funcionarios");
    }

    public async Task<bool> AtualizarFuncionarioAsync(FuncionarioModel funcionario)
    {
      var response = await _httpClient.PutAsJsonAsync($"http://localhost:5235/api/Funcionario/atualizar", funcionario);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletarFuncionarioAsync(Guid funcionarioId)
    {
      var response = await _httpClient.DeleteAsync($"http://localhost:5235/api/Funcionario/deletar/{funcionarioId}");
      return response.IsSuccessStatusCode;
    }
  }

}
