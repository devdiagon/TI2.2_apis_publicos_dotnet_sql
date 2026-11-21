using System.Text.Encodings.Web;
using System.Text.Unicode;
using ec.edu.monster.modelo;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("SqlServerConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:SqlServerConnection en appsettings.json");
builder.Services.AddDbContext<AppDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));
builder.Services.AddScoped<PaisRepositorio>();

builder.Services.Configure<RestCountriesOpciones>(builder.Configuration.GetSection("RestCountries"));
builder.Services.AddHttpClient<ConsumoApi>(cliente => cliente.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddControllersWithViews();
builder.Services.Configure<RazorViewEngineOptions>(opciones =>
{
    opciones.ViewLocationFormats.Clear();
    opciones.ViewLocationFormats.Add("/ec/edu/monster/vista/{0}.cshtml");
});

builder.Services.AddSingleton(HtmlEncoder.Create(new TextEncoderSettings(
    UnicodeRanges.BasicLatin,
    UnicodeRanges.Latin1Supplement,
    UnicodeRanges.GeneralPunctuation,
    UnicodeRanges.Arrows)));

var app = builder.Build();

app.UsePathBase("/apis-publicos-dotnet");

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("es-EC")
    .AddSupportedCultures("es-EC")
    .AddSupportedUICultures("es-EC"));

app.UseStaticFiles();
app.UseRouting();

app.MapGet("/", () => Results.Redirect("~/paises"));
app.MapControllers();

app.Run();
