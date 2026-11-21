using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Vistas Razor en una carpeta plana: ec/edu/monster/vista/{nombre}.cshtml
builder.Services.AddControllersWithViews();
builder.Services.Configure<RazorViewEngineOptions>(opciones =>
{
    opciones.ViewLocationFormats.Clear();
    opciones.ViewLocationFormats.Add("/ec/edu/monster/vista/{0}.cshtml");
});

// Tildes, «ñ», «…» y «→» sin entidades HTML; <, >, & y comillas siguen escapándose.
builder.Services.AddSingleton(HtmlEncoder.Create(new TextEncoderSettings(
    UnicodeRanges.BasicLatin,
    UnicodeRanges.Latin1Supplement,
    UnicodeRanges.GeneralPunctuation,
    UnicodeRanges.Arrows)));

var app = builder.Build();

// Números y fechas con la configuración regional de Ecuador (1.429.404.000).
var cultura = new CultureInfo("es-EC");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

// Equivalente al context root de la versión Java: toda la aplicación vive bajo /apis-publicos-dotnet.
app.UsePathBase("/apis-publicos-dotnet");

app.UseStaticFiles();
app.UseRouting();

// El inicio es la lista de países.
app.MapGet("/", () => Results.Redirect("~/paises"));
app.MapControllers();

app.Run();
