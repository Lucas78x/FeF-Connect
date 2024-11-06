using AspnetCoreMvcFull.Enums;

namespace AspnetCoreMvcFull.DTO
{
  public class EventoDTO
  {
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTime Data { get; set; }
    public string Local { get; set; }
    public string Link { get; set; }
    public TipoPermissaoEnum Setor { get; set; }

    public EventoDTO(Guid id, string titulo, string descricao, DateTime data, string local, string link, int setor)
    {
      Id = id;
      Titulo = titulo;
      Descricao = descricao;
      Data = data;
      Local = local;
      Link = link;
      Setor = (TipoPermissaoEnum)setor;
    }
    public EventoDTO()
    {

    }
  }
}
