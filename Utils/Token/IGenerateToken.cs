namespace AspnetCoreMvcFull.Utils.Token
{
  public interface IGenerateToken
  {
    string GenerateJwtToken(string username);
  }
}
