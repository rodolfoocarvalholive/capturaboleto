var builder = WebApplication.CreateBuilder(args);

// Habilita arquivos estáticos
builder.Services.AddDirectoryBrowser();
var app = builder.Build();

// Usa arquivos estáticos do wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
