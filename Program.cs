using AspnetCoreMvcFull.Utils;
using AspnetCoreMvcFull.Utils.PDF;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IValidateSession, ValidateSession>();
builder.Services.AddScoped<IFileEncryptor, FileEncryptor>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=LoginBasic}/{id?}");

app.Run();
