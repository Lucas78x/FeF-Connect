using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  public class PersonController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IFileEncryptor _fileEncryptor;

    public PersonController(IValidateSession validateSession, IConfiguration configuration, IFileEncryptor fileEncryptor)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _fileEncryptor = fileEncryptor;
    }

    public async Task<IActionResult> Pessoal()
    {
      if (_validateSession.IsUserValid())
      {
        var handler = new HttpClientHandler
        {
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        var client = new HttpClient(handler);
        var id = HttpContext.Session.GetInt32("Id") ?? -1;

        try
        {

          client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

          var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");
          if (response.IsSuccessStatusCode)
          {
            var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioModel>();
            if (funcionario != null)
            {
              return View("Pessoal",funcionario);
            }
            else
            {

              return RedirectToPage("MiscError", "Pages");
            }
          }
        }
        catch
        {
          return View("MiscError", "Pages");

        }
      }
      return View("LoginBasic", "Auth");
    }
    public IActionResult RedirecToPerson()
    {
      if (_validateSession.IsUserValid())
      {
        return RedirectToAction("Pessoal", "Pessoal");
      }
      else
      {
        return View("LoginBasic", "Auth");
      }
    }
  }
}
