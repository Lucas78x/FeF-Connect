namespace AspnetCoreMvcFull.Models
{
  public class Link
  {
    public string Nome { get; set; }
    public string Url { get; set; }
    public string Descricao { get; set; }
  }
  public class LinksModel
  {
    public Dictionary<string, List<Link>> Links { get; set; }
  }
}
