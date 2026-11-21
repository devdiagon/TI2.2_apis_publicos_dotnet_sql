using System.Net.Http.Headers;
using System.Text.Json;
using ec.edu.monster.excepciones;
using Microsoft.Extensions.Options;

namespace ec.edu.monster.modelo;

public class UniversidadesOpciones
{
    public string BaseUrl { get; set; } = "http://universities.hipolabs.com";
}

public class ConsumoApiUniversidades
{
    private const string MsgRed = "El servicio de universidades no está disponible, intente más tarde";
    private const string MsgIlegible = "No se pudo interpretar la respuesta del servicio de universidades";

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<UniversidadesOpciones> _opciones;

    public ConsumoApiUniversidades(HttpClient http, IOptionsMonitor<UniversidadesOpciones> opciones)
    {
        _http = http;
        _opciones = opciones;
    }

    public async Task<List<Universidad>> BuscarPorPaisAsync(string pais, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pais))
        {
            throw new ExcepcionMonster("Escriba el nombre del país");
        }
        var url = $"{_opciones.CurrentValue.BaseUrl.TrimEnd('/')}/search?country={Uri.EscapeDataString(pais.Trim())}";

        using var peticion = new HttpRequestMessage(HttpMethod.Get, url);
        peticion.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        try
        {
            using var respuesta = await _http.SendAsync(peticion, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                throw new ExcepcionMonster(MsgIlegible);
            }
            return Convertir(await respuesta.Content.ReadAsStringAsync(ct));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ExcepcionMonster(MsgRed, e);
        }
    }
    
    public static List<Universidad> Convertir(string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            return documento.RootElement.EnumerateArray().Select(AUniversidad).ToList();
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException
                                      or KeyNotFoundException or FormatException)
        {
            throw new ExcepcionMonster(MsgIlegible, e);
        }
    }

    private static Universidad AUniversidad(JsonElement objeto) => new()
    {
        Nombre = Recortar(Texto(objeto, "name"), Universidad.LongitudNombre)
                 ?? throw new InvalidOperationException("Universidad sin nombre"),
        Dominio = Recortar(PrimerTexto(objeto, "domains"), Universidad.LongitudDominio),
        SitioWeb = Recortar(PrimerTexto(objeto, "web_pages"), Universidad.LongitudSitioWeb),
        Pais = Recortar(Texto(objeto, "country"), Universidad.LongitudPais)
               ?? throw new InvalidOperationException("Universidad sin país"),
    };

    private static string? Texto(JsonElement objeto, string campo) =>
        objeto.TryGetProperty(campo, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static string? PrimerTexto(JsonElement objeto, string campo)
    {
        if (!objeto.TryGetProperty(campo, out var lista) || lista.ValueKind != JsonValueKind.Array)
        {
            return null;
        }
        return lista.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
    }

    private static string? Recortar(string? valor, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }
        var limpio = valor.Trim();
        return limpio.Length > maximo ? limpio[..maximo] : limpio;
    }
}
