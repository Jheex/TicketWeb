using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PIM.Data;

var builder = WebApplication.CreateBuilder(args);

// Conexão com o SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PIMConnection")));

// Adicionar serviços MVC
builder.Services.AddControllersWithViews();

// Configurar autenticação por cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";      // rota para login
        options.AccessDeniedPath = "/Account/Login"; // rota se acesso negado
    });

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Rota padrão iniciando na página de login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
