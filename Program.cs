using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.Utils.Email;
using AspnetCoreMvcFull.Utils.Funcionarios;
using AspnetCoreMvcFull.Utils.Messagem;
using AspnetCoreMvcFull.Utils.PDF;
using AspnetCoreMvcFull.Utils.Token;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.HttpOverrides; 
using Serilog;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserIdProvider, SessionUserIdProvider>();
builder.Services.AddScoped<IValidateSession, ValidateSession>();
builder.Services.AddScoped<IFileEncryptor, FileEncryptor>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGenerateToken, GenerateToken>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Session configuration
builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromHours(2);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
  options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();


app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();
app.UseSession();

// Default route and SignalR Hub mapping
app.UseEndpoints(endpoints =>
{
  // Default route for controllers
  endpoints.MapControllerRoute(
      name: "default",
      pattern: "{controller=Auth}/{action=LoginBasic}/{id?}"
  );

  // SignalR Hub
  endpoints.MapHub<ChatHub>("/chatHub");
});

// Run the application
app.Run();
