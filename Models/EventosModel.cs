using AspnetCoreMvcFull.Enums;
using Ganss.Xss;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class EventoModel
  {
    public Guid Id { get; set; }
    public string Titulo { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Descricao { get; set; }

    [Required(ErrorMessage = "A data é obrigatória.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Required(ErrorMessage = "O local é obrigatório.")]
    public string Local { get; set; }

    [Url(ErrorMessage = "O link deve ser uma URL válida.")]
    public string Link { get; set; }

    public TipoPermissaoEnum Setor { get; set; }

    public void SetId()
    {
      Id = Guid.NewGuid();
    }
    public void SetSetor(TipoPermissaoEnum setor)
    {
      Setor = setor;
    }
    public void Sanitizar()
    {
      var sanitizer = new HtmlSanitizer();
      Descricao = sanitizer.Sanitize(Descricao);
      Link = sanitizer.Sanitize(Link);
      Local = sanitizer.Sanitize(Local);
    }
    public EventoModel(string titulo, string descricao, DateTime data, string local, string link, int setor)
    {
      Id = Guid.NewGuid();
      Titulo = titulo;
      Descricao = descricao;
      Data = data;
      Local = local;
      Link = link;
      Setor = (TipoPermissaoEnum)setor;
    }
    public EventoModel()
    {

    }
  }
}
