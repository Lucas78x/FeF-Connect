using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;

namespace AspnetCoreMvcFull.Utils
{

  public class FileEncryptor : IFileEncryptor
  {
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;

    public FileEncryptor(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
      _webHostEnvironment = webHostEnvironment;
      _configuration = configuration;
    }

    private static byte[] GenerateKey(string password, byte[] salt)
    {
      using (var keyGenerator = new Rfc2898DeriveBytes(password, salt, 10000))
      {
        return keyGenerator.GetBytes(32);
      }
    }
    public string GetRootPath()
    {
      return _webHostEnvironment.WebRootPath;
    }

    public string GetSecretKey()
    {
      return _configuration["Authentication:Secret"];
    }

    public void EncryptFile(string folderPath, string password)
    {

      byte[] salt = new byte[8];
      using (var rng = new RNGCryptoServiceProvider())
      {
        rng.GetBytes(salt);
      }

      using (var aes = new AesManaged())
      {
        aes.KeySize = 256;
        aes.BlockSize = 128;
        var key = new Rfc2898DeriveBytes(password, salt, 1000).GetBytes(32);
        aes.Key = key;
        aes.IV = new byte[16];
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CBC;

        foreach (string filePath in Directory.GetFiles(folderPath))
        {
          string encryptedFilePath = filePath + ".enc"; // Create a new file with .enc extension

          using (var fsIn = new FileStream(filePath, FileMode.Open, FileAccess.Read))
          {
            using (var fsOut = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write))
            {
              fsOut.Write(salt, 0, salt.Length);

              using (var cryptoStream = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
              {
                fsIn.CopyTo(cryptoStream);
              }
            }
          }

          File.Delete(filePath);
        }
      }
    }

    public void DecryptFile(string folderPath, string password)
    {
      foreach (string filePath in Directory.GetFiles(folderPath, "*.enc"))
      {
        string decryptedFilePath = filePath.Substring(0, filePath.Length - 4); // Remove .enc extension

        using (var fsIn = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
          byte[] salt = new byte[8];
          fsIn.Read(salt, 0, salt.Length);

          using (var aes = new AesManaged())
          {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(password, salt, 1000).GetBytes(32);
            aes.Key = key;
            aes.IV = new byte[16];
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            using (var fsOut = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write))
            {
              using (var cryptoStream = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
              {
                cryptoStream.CopyTo(fsOut);
              }
            }
          }
        }

        File.Delete(filePath);
      }
    }

    public string UserIconUrl(string patch, string oldPatch, string password, int Id, TipoGeneroEnum Genero)
    {
      string userDirectory = Path.Combine(patch, Id.ToString());

      if (!Directory.Exists(patch))
      {

        Directory.CreateDirectory(patch);
        //EncryptFile(patch, password);
      }

      if (!Directory.Exists(userDirectory))
      {
        //DecryptFile(patch, password);
        Directory.CreateDirectory(userDirectory);
       

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


