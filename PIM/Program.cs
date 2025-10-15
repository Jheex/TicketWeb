using Microsoft.AspNetCore.Authentication.Cookies; // Biblioteca para autenticação via cookie
using Microsoft.EntityFrameworkCore;               // Biblioteca do Entity Framework Core
using PIM.Data;                                     // Namespace do seu DbContext (AppDbContext)


var builder = WebApplication.CreateBuilder(args);   // Cria o builder da aplicação

// Configura o DbContext para conectar ao SQL Server usando a connection string definida em appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PIMConnection")));

// Adiciona suporte a controllers e views (MVC)
builder.Services.AddControllersWithViews();

// Configura autenticação usando cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Caminho para a página de login quando o usuário não estiver autenticado
        options.LoginPath = "/Account/Login";

        // Caminho caso o usuário tente acessar um recurso sem permissão
        options.AccessDeniedPath = "/Account/Login";
    });


var app = builder.Build(); // Constrói o app final


// Configurações para ambientes que NÃO são de desenvolvimento
if (!app.Environment.IsDevelopment())
{
    // Página de erro personalizada
    app.UseExceptionHandler("/Home/Error");

    // Habilita HSTS (HTTP Strict Transport Security) para produção
    app.UseHsts();
}

// Redireciona todas as requisições HTTP para HTTPS
app.UseHttpsRedirection();

// Permite servir arquivos estáticos (css, js, imagens, etc.)
app.UseStaticFiles();

// Habilita o roteamento da aplicação
app.UseRouting();

// Habilita autenticação (cookies)
app.UseAuthentication();

// Habilita autorização (controle de acesso)
app.UseAuthorization();

// Define a rota padrão da aplicação
// Aqui, a página inicial será Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Executa a aplicação
app.Run();
