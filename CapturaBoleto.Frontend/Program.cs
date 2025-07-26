var builder = WebApplication.CreateBuilder(args);

// Habilita arquivos est�ticos
builder.Services.AddDirectoryBrowser();
var app = builder.Build();

// Usa arquivos est�ticos do wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
