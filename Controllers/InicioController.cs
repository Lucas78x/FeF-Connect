using AspnetCoreMvcFull.DTO;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static AspnetCoreMvcFull.Controllers.RamaisController;
using System.Net.Http;
using AspnetCoreMvcFull.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Newtonsoft.Json;
using AspnetCoreMvcFull.Enums;
using OxyPlot;
using System.Text;

namespace AspnetCoreMvcFull.Controllers
{
  public class InicioController : Controller
  {
    private readonly IValidateSession _validateSession;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileEncryptor _fileEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentController> _logger;
    private static readonly HttpClient client = new HttpClient();
    private readonly string _apiKey;
    private readonly string _CityId;
    private const string BaseUrl = "http://api.openweathermap.org/data/2.5/forecast";

    public InicioController(IValidateSession validateSession, IConfiguration configuration,
                                  IWebHostEnvironment webHostEnvironment, IFileEncryptor fileEncryptor,
                                  IHttpClientFactory httpClientFactory, ILogger<DocumentController> logger)
    {
      _validateSession = validateSession;
      _configuration = configuration;
      _webHostEnvironment = webHostEnvironment;
      _fileEncryptor = fileEncryptor;
      _httpClientFactory = httpClientFactory;
      _logger = logger;

      _apiKey = _configuration["Authentication:ForecastKey"];
      _CityId = _configuration["Authentication:CityId"];
    }


    public async Task<IActionResult> Inicio()
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      var client = _httpClientFactory.CreateClient();
      client.DefaultRequestHeaders.Authorization =
          new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Authentication:TokenKey"]);

      var id = HttpContext.Session.GetInt32("Id") ?? -1;

      try
      {
        var response = await client.GetAsync($"http://localhost:5235/api/Auth/funcionario?Id={id}");
        if (response.IsSuccessStatusCode)
        {
          var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioModel>();
          if (funcionario == null)
            return RedirectToAction("MiscError", "MiscError");

          var funcionarios = await client.GetFromJsonAsync<List<FuncionarioModel>>("http://localhost:5235/api/Auth/funcionarios");
          var setorId = funcionario.Permissao.GetHashCode();
          var eventoModel = await client.GetFromJsonAsync<List<EventoModel>>($"http://localhost:5235/api/Eventos/eventos?Setor={setorId}");
          var comunicadosModel = await client.GetFromJsonAsync<List<ComunicadosModel>>($"http://localhost:5235/api/Comunicados/comunicados");

          UpdateSessionWithFuncionario(funcionario);
          var happyBirthFunc = new List<FuncionarioModel>();
          if (funcionarios != null)
          {
            int currentMonth = DateTime.Now.Month;
            happyBirthFunc = funcionarios.Where(x => x.DataNascimento.Month == currentMonth).ToList();

          }

          string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dicas", "Dicas.json"); 

          string jsonString = await System.IO.File.ReadAllTextAsync(jsonFilePath);
          DicasResponse Dicas = new DicasResponse();
          Random random = new Random();
          var dicaAleatoria = new DicasModel();

          if (!string.IsNullOrEmpty(jsonString))
          {
            Dicas = JsonConvert.DeserializeObject<DicasResponse>(jsonString);
            if (Dicas.dicas.Any())
            {
              dicaAleatoria = Dicas.dicas[random.Next(Dicas.dicas.Count)];
            }
          }
          var partialModel = new PartialModel
          {
            front = new FrontModel(UserIcon()),
            funcionario = funcionario,
            Funcionarios = happyBirthFunc,
            Forecast = GetForecastInformation().Result,
            Eventos = eventoModel,
            Dica = dicaAleatoria,
            Comunicados = comunicadosModel,
            Noticias = GetNoticias().Result
          };

          return View("Inicio", partialModel);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching funcionario data");
        return View("MiscError", "MiscError");
      }

      return RedirectToAction("LoginBasic", "Auth");
    }
    [HttpPost]
    [Route("Inicio/AddComunicado")]
    public async Task<IActionResult> AddComunicado([FromBody] ComunicadosModel comunicados)
    {
      if (ModelState.IsValid)
      {
        try
        {
          comunicados.Sanitizar();

          // Post the comunicado to the API
          var response = await client.PostAsJsonAsync("http://localhost:5235/api/Comunicados/addcomunicado", comunicados);

          if (response.IsSuccessStatusCode)
          {

            return Json(new { success = true});
          }


          return Json(new { success = false});
        }
        catch (Exception ex)
        {

          return Json(new { success = false});
        }
      }
      return Json(new { success = false });
    }

    [HttpPost]
    [Route("Inicio/AddEvento")]
    public async Task<IActionResult> AddEvento([FromBody] EventoModel eventoModel, string setor)
    {
      if (ModelState.IsValid)
      {
        try
        {
          eventoModel.SetId();
          TipoPermissaoEnum? tipoPermissao = EnumExtensions.StringToEnum<TipoPermissaoEnum>(setor);

          eventoModel.SetSetor(tipoPermissao.Value);
          eventoModel.Sanitizar();

          var requestBody = new EventoDTO
          {
            Id = eventoModel.Id,
            Titulo = "", // Defina um título se necessário
            Descricao = eventoModel.Descricao,
            Data = eventoModel.Data,
            Local = eventoModel.Local,
            Link = "", // Defina um link se necessário
            Setor = eventoModel.Setor // Certifique-se de que eventoModel.Setor é do tipo correto
          };

          var response = await client.PostAsJsonAsync("http://localhost:5235/api/Eventos/addevento",requestBody);

          if (response.IsSuccessStatusCode)
          {

            return Json(new { success = true });
          }


          return Json(new { success = false });
        }
        catch (Exception ex)
        {

          return Json(new { success = false });
        }
      }
      return Json(new { success = false });
    }
    public async Task<ForecastModel> GetForecastInformation()
    {

      try
      {

        // Configurando o HttpClient
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        string url = $"{BaseUrl}?id={_CityId}&appid={_apiKey}&lang=pt&units=metric";

        var resposta = await client.GetAsync(url);
        resposta.EnsureSuccessStatusCode(); // Garante que a resposta foi bem-sucedida

        var conteudo = await resposta.Content.ReadAsStringAsync();
        var dados = JObject.Parse(conteudo);


        foreach (var item in dados["list"])
        {
          var data = item["dt_txt"].Value<string>();
          var temperatura = item["main"]["temp"].Value<int>();
          var descricao = item["weather"][0]["description"].Value<string>();
          var umidade = item["main"]["humidity"].Value<int>();
          var pop = item["pop"].Value<double>();
          var chuva = item["rain"]?["1h"]?.Value<double>() ?? 0;

          if (chuva.Equals("0"))
          {
            return new ForecastModel(temperatura.ToString(), descricao, umidade.ToString(), pop.ToString());
          }
          else
          {
            return new ForecastModel(temperatura.ToString(), descricao, umidade.ToString(), chuva.ToString());
          }
        }
      }
      catch (HttpRequestException e)
      {
        Console.WriteLine($"Erro ao obter dados: {e.Message}");
        if (e.StatusCode != null)
        {
          Console.WriteLine($"Código de status: {e.StatusCode}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro inesperado: {ex.Message}");
      }

      return new ForecastModel();
    }
    public async Task<List<NoticiasModel>> GetNoticias()
    {
      string apiKey = "ccc79af78afb43818bd1152abc22b614"; // Substitua pela sua chave de API
      string query = "tecnologia";
      string language = "pt";

      // Codificando a URL
      string url = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(query)}&language={language}&apiKey={apiKey}";
      var keywords = new[] { "internet", "provedor de internet", "banda larga", "conexão", "wifi", "rede" };

      try
      {
        // Adicionando cabeçalho User-Agent
        client.DefaultRequestHeaders.Add("User-Agent", "YourAppName");

        var response = await client.GetStringAsync(url);
        JObject json = JObject.Parse(response);
        var noticiasModel = new List<NoticiasModel>();

        if (json["status"].ToString() == "ok")
        {
          foreach (var article in json["articles"])
          {
            NoticiasModel not = new();
            not.Title = article["title"].ToString();

            // Verifica se o título contém alguma das palavras-chave
            //if (keywords.Any(keyword => not.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            //{
              not.Url = article["url"].ToString();
              noticiasModel.Add(not);
            //}
          }
        }
        else
        {
          Console.WriteLine("Erro ao buscar notícias: " + json["message"]);
        }
        return noticiasModel;
      }
      catch (HttpRequestException ex)
      {
        Console.WriteLine("Erro de requisição: " + ex.Message);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Erro: " + ex.Message);
      }
      return new List<NoticiasModel>();
    }
    private void UpdateSessionWithFuncionario(FuncionarioModel funcionario)
    {
      _validateSession.SetFuncionarioId(funcionario.Id);
      _validateSession.SetFuncionarioPermissao(funcionario.Permissao.GetHashCode());
    }
    private string UserIcon()
    {
      string patch = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["Authentication:UserPatch"]);
      string key = _configuration["Authentication:Secret"];

      ViewBag.Username = _validateSession.GetUsername();
      return _fileEncryptor.UserIconUrl(patch, _webHostEnvironment.WebRootPath, key, _validateSession.GetUserId(), _validateSession.GetGenero());
    }

    [HttpPost]
    public async Task<IActionResult> RedirecToInicio()
    {
      if (!_validateSession.IsUserValid())
        return RedirectToAction("LoginBasic", "Auth");

      return RedirectToAction("Inicio", "Inicio");
    }
    public class DicasResponse
    {
      public List<DicasModel> dicas { get; set; }
    }
  }


}
