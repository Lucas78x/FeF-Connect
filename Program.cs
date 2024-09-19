using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.Utils.Email;
using AspnetCoreMvcFull.Utils.PDF;
using Microsoft.AspNetCore.SignalR;
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
builder.Services.AddScoped<HttpClient>();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromHours(2); 
  options.Cookie.HttpOnly = true; 
  options.Cookie.IsEssential = true; 
});


var app = builder.Build();

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

app.UseEndpoints(endpoints =>
{
  // Default route for controllers
  endpoints.MapControllerRoute(
      name: "default",
      pattern: "{controller=Auth}/{action=LoginBasic}/{id?}"
  );

  endpoints.MapHub<ChatHub>("/chatHub");
});

app.Run();
