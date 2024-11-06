using AspnetCoreMvcFull.DTO;

namespace AspnetCoreMvcFull.Utils.Funcionarios
{
  public interface IFuncionarioService
  {
    Task<FuncionarioModel> ObterFuncionarioPorIdAsync(Guid funcionarioId);
    Task<List<FuncionarioModel>> ObterTodosFuncionariosAsync();
    Task<bool> AtualizarFuncionarioAsync(FuncionarioModel funcionario);
    Task<bool> DeletarFuncionarioAsync(Guid funcionarioId);
  }

}
