using Ganss.Xss;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class ComunicadosModel
  {
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres.")]
    public string Titulo { get; set; }

    [Required(ErrorMessage = "A mensagem é obrigatória.")]
    [StringLength(500, ErrorMessage = "A mensagem deve ter no máximo 500 caracteres.")]
    public string Mensagem { get; set; }

    [Required(ErrorMessage = "A data de publicação é obrigatória.")]
    public DateTime DataPublicacao { get; set; }
    public bool Ativo { get; set; }
    public void Sanitizar()
    {
      var sanitizer = new HtmlSanitizer();
      Titulo = sanitizer.Sanitize(Titulo);
      Mensagem = sanitizer.Sanitize(Mensagem);
    }
    public ComunicadosModel(string titulo, string mensagem, DateTime dataPublicacao, bool ativo = true)
    {
      Id = Guid.NewGuid();
      Titulo = titulo;
      Mensagem = mensagem;
      DataPublicacao = dataPublicacao;
      Ativo = ativo;
    }
  }
}
