using AspnetCoreMvcFull.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{

  public class RegisterModel
  {
    public long Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome deve ter no máximo 50 caracteres.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O sobrenome é obrigatório.")]
    [StringLength(50, ErrorMessage = "O sobrenome deve ter no máximo 50 caracteres.")]
    public string Sobrenome { get; set; }

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    public string CPF { get; set; }

    [Required(ErrorMessage = "O RG é obrigatório.")]
    [StringLength(12, ErrorMessage = "O RG deve ter no máximo 12 caracteres.")]
    public string RG { get; set; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    public string Email { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public TipoGeneroEnum Genero { get; set; }

    public DateTime DataNascimento { get; set; }

    public string Cargo { get; set; }

    public TipoPermissaoEnum Permissao { get; set; }
  }

}
