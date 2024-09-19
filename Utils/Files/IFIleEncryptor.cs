using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Utils
{
  public interface IFileEncryptor
  {
    void EncryptFile(string filePath, string password);
    void DecryptFile(string filePath, string password);
    string UserIconUrl(string patch, string oldPatch, string password, int Id, TipoGeneroEnum Genero);
    List<PayslipModel> UserCheque(string patch, string oldPatch, string password, int id);
    string UserEscala(string patch, string oldPatch, string password);
    Task ChangePhoto(IFormFile photoUpload, string patch, string password, int Id);

  }

}
