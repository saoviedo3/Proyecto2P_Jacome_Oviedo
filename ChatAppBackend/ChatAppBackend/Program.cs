using Microsoft.EntityFrameworkCore;
using ChatAppBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// Habilitar servicios de controladores
builder.Services.AddControllers();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configurar conexión a MySQL
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 32))));

// Agregar soporte para sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Agregar SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseWebSockets();
app.UseSession();
app.UseRouting();
app.UseCors();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
    endpoints.MapControllers();
});
app.UseAuthorization();
app.Run();
