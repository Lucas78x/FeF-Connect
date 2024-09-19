namespace AspnetCoreMvcFull.DTO
{
  public class LoginModel
  {
    public long Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Guid FuncionarioId { get; set; }
    public FuncionarioModel Funcionario { get; set; }
    public LoginModel() { }

    public LoginModel(string username, string passwordHash, FuncionarioModel funcionario)
    {

      Username = username;
      PasswordHash = passwordHash;
      FuncionarioId = funcionario.Id;
      Funcionario = funcionario;
    }
  }
}
