using AspnetCoreMvcFull.Enums;

namespace AspnetCoreMvcFull.DTO
{
  public class FuncionarioDTO
  {
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string CPF { get; set; }
    public string RG { get; set; }
    public TipoGeneroEnum Genero { get; set; }
    public string Email { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Cargo { get; set; }
    public TipoPermissaoEnum Permissao { get; set; }
    public FuncionarioDTO() { }

    public FuncionarioDTO(string nome, string sobrenome, string cpf, string rg, TipoGeneroEnum genero, string email, DateTime dataNascimento, string cargo, TipoPermissaoEnum permissao)
    {
      Id = Guid.NewGuid();
      Nome = nome;
      Sobrenome = sobrenome;
      CPF = cpf;
      RG = rg;
      Genero = genero;
      Email = email;
      DataNascimento = dataNascimento;
      Cargo = cargo;
      Permissao = permissao;

    }

  }

}
