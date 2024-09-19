using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using System.Security.Cryptography;

namespace AspnetCoreMvcFull.Utils
{

  public class FileEncryptor : IFileEncryptor
  {
    private static byte[] GenerateKey(string password, byte[] salt)
    {
      using (var keyGenerator = new Rfc2898DeriveBytes(password, salt, 10000))
      {
        return keyGenerator.GetBytes(32);
      }
    }

    public void EncryptFile(string filePath, string password)
    {
      byte[] salt = new byte[16];
      using (var rng = new RNGCryptoServiceProvider())
      {
        rng.GetBytes(salt);
      }

      byte[] key = GenerateKey(password, salt);

      using (var aes = Aes.Create())
      {
        aes.Key = key;
        aes.GenerateIV();

        using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
        {
          fileStream.Write(salt, 0, salt.Length);
          fileStream.Write(aes.IV, 0, aes.IV.Length);

          using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
          using (var inputFileStream = new FileStream(filePath, FileMode.Open))
          {
            inputFileStream.CopyTo(cryptoStream);
          }
        }
      }
    }

    public void DecryptFile(string filePath, string password)
    {
      using (var fileStream = new FileStream(filePath, FileMode.Open))
      {
        byte[] salt = new byte[16];
        fileStream.Read(salt, 0, salt.Length);

        byte[] key = GenerateKey(password, salt);

        using (var aes = Aes.Create())
        {
          byte[] iv = new byte[aes.IV.Length];
          fileStream.Read(iv, 0, iv.Length);

          aes.Key = key;
          aes.IV = iv;

          using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
          using (var outputFileStream = new FileStream(filePath + ".decrypted", FileMode.Create))
          {
            cryptoStream.CopyTo(outputFileStream);
          }
        }
      }
    }

    public string UserIconUrl(string patch,string oldPatch, string password, int Id, TipoGeneroEnum Genero)
    {
      string userDirectory = Path.Combine(patch, Id.ToString());

      if (!Directory.Exists(patch))
      {
        
        Directory.CreateDirectory(patch);
       // EncryptFile(patch, password);
      }

      if (!Directory.Exists(userDirectory))
      {
        //DecryptFile(patch, password);
        Directory.CreateDirectory(userDirectory);
        //EncryptFile(patch, password);

        switch (Genero)
        {
          case TipoGeneroEnum.None:
            break;
          case TipoGeneroEnum.Masculino:
            return "~/Users/Partial/Male.png";
          case TipoGeneroEnum.Feminino:
            return "~/Users/Partial/Female.png";
          case TipoGeneroEnum.GN:
            break;
        }
      }

      //DecryptFile(patch, password);

      var latestFile = new DirectoryInfo(userDirectory).GetFiles()
                        .Where(f => f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                    f.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                    f.Extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                    f.Extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                                    f.Extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(f => f.LastWriteTime)
                        .FirstOrDefault();

      //EncryptFile(patch, password);

      if (latestFile != null)
      {
        string absolutePath = Path.Combine(userDirectory, latestFile.Name);
        string relativePath = absolutePath.Replace(oldPatch, "~");
        relativePath = relativePath.Replace("\\", "/");

        return relativePath;
      }

      switch (Genero)
      {
        case TipoGeneroEnum.Masculino:
          return "~/Users/Partial/Male.png";
        case TipoGeneroEnum.Feminino:
          return "~/Users/Partial/Female.png";
        default:
          return "~/Users/Partial/Default.png";
      }
    }
    public List<PayslipModel> UserCheque(string patch, string oldPatch, string password, int id)
    {
      string userDirectory = Path.Combine(patch, id.ToString());
      string chequeDirectory = Path.Combine(userDirectory, "Cheque");

      EnsureDirectoryExists(patch, password);
      EnsureDirectoryExists(userDirectory);
      EnsureDirectoryExists(chequeDirectory);

      var payslipFiles = new DirectoryInfo(chequeDirectory).GetFiles("*.pdf")
                          .OrderByDescending(f => f.LastWriteTime)
                          .ToList();

      var payslips = new List<PayslipModel>();

      foreach (var file in payslipFiles)
      {
    
        string relativePath = file.FullName.Replace(oldPatch, "~").Replace("\\", "/");

        payslips.Add(new PayslipModel
        {
          Url = relativePath,
          Date = file.LastWriteTime
        });
      }

      return payslips;
    }
    public PayslipModel UserCheque(string patch, string oldPatch, string password, int id, DateTime? filterDate)
    {
      string userDirectory = Path.Combine(patch, id.ToString());
      string chequeDirectory = Path.Combine(userDirectory, "Cheque");

      EnsureDirectoryExists(patch, password);
      EnsureDirectoryExists(userDirectory);
      EnsureDirectoryExists(chequeDirectory);

      var payslipFiles = new DirectoryInfo(chequeDirectory).GetFiles("*.pdf")
                            .OrderByDescending(f => f.LastWriteTime)
                            .ToList();

      var matchingFile = payslipFiles
          .FirstOrDefault(file =>
              (!filterDate.HasValue) ||
              (file.LastWriteTime.Year == filterDate.Value.Year &&
               file.LastWriteTime.Month == filterDate.Value.Month &&
               file.LastWriteTime.Day == filterDate.Value.Day &&
               file.LastWriteTime.Hour == filterDate.Value.Hour &&
               file.LastWriteTime.Minute == filterDate.Value.Minute));

      if (matchingFile != null)
      {
        string relativePath = matchingFile.FullName.Replace(oldPatch, "~").Replace("\\", "/");

        return new PayslipModel
        {
          Url = relativePath,
          Date = matchingFile.LastWriteTime
        };
      }

      return null; 
    }

   
    private void EnsureDirectoryExists(string path, string password = null)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);      
      }
    }

    public string UserEscala(string patch, string oldPatch, string password)
    {
      string userDirectory = Path.Combine(patch, "Partial"); // TODO: Externalizar
      string escalaDirectory = Path.Combine(userDirectory, "Escala"); //TODO: Externalizar

      if (!Directory.Exists(patch))
      {

        Directory.CreateDirectory(patch);
        // EncryptFile(patch, password);
      }

      if (!Directory.Exists(userDirectory))
      {
        Directory.CreateDirectory(userDirectory);
      }

      if (!Directory.Exists(escalaDirectory))
      {
        Directory.CreateDirectory(escalaDirectory);
      }

      var latestFile = new DirectoryInfo(escalaDirectory).GetFiles()
                       .OrderByDescending(f => f.LastWriteTime)
                       .FirstOrDefault();
      if (latestFile != null)
      {
        string absolutePath = Path.Combine(escalaDirectory, latestFile.Name);
        string relativePath = absolutePath.Replace(oldPatch, "~");
        relativePath = relativePath.Replace("\\", "/");

        return relativePath;
      }

      return null;
    }
    public async Task ChangePhoto(IFormFile photoUpload, string patch, string password, int Id)
    {
      // Verifica se o arquivo foi enviado
      if (photoUpload == null || photoUpload.Length == 0)
      {
        throw new ArgumentException("Nenhum arquivo enviado.");
      }

      var userDirectory = Path.Combine(patch, Id.ToString());

      if (!Directory.Exists(patch))
      {
        Directory.CreateDirectory(patch);
      }

      if (!Directory.Exists(userDirectory))
      {
        Directory.CreateDirectory(userDirectory);
      }

      var fileName = Path.GetFileName(photoUpload.FileName);
      var filePath = Path.Combine(userDirectory, fileName);


      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await photoUpload.CopyToAsync(stream);
      }
    }

  }
}


