using AspnetCoreMvcFull.Enums;

namespace AspnetCoreMvcFull.Utils
{
  public interface IFileEncryptor
  {
    void EncryptFile(string filePath, string password);
    void DecryptFile(string filePath, string password);
    string UserIconUrl(string patch,string oldPatch, string password, int Id, TipoGeneroEnum Genero);
    string UserContraCheque(string patch, string oldPatch, string password, int Id);
    string UserEscala(string patch, string oldPatch, string password);
    Task ChangePhoto(IFormFile photoUpload, string patch, string password, int Id);

  } 

}
