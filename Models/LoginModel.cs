using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class LoginModel
  {
    public long Id { get; set; }
    public int Genero { get; set; }

    [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
    public string PasswordHash { get; set; }
  }
}
