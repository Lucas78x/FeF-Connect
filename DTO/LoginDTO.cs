namespace AspnetCoreMvcFull.DTO
{
  public class LoginDTO
  {
    public long Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Guid FuncionarioId { get; set; }
    public FuncionarioDTO Funcionario { get; set; }
    public LoginDTO() { }

    public LoginDTO(string username, string passwordHash, FuncionarioDTO funcionario)
    {

      Username = username;
      PasswordHash = passwordHash;
      FuncionarioId = funcionario.Id;
      Funcionario = funcionario;
    }
  }
}
