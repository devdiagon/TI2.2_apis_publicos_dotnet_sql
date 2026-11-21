using System.Net.Http.Headers;
using System.Text.Json;
using ec.edu.monster.excepciones;
using Microsoft.Extensions.Options;

namespace ec.edu.monster.modelo;

public class RestCountriesOpciones
{
    public string BaseUrl { get; set; } = "https://api.restcountries.com/countries/v5";
    public string ApiKey { get; set; } = "";
}

public class ConsumoApi
{
    private const string Campos = "response_fields=names.common,capitals,region,subregion,population,area,currencies";
    private const int TamanoPagina = 100;
    private const int MaximoPaginas = 20;

    private const string MsgRed = "El servicio de países no está disponible, intente más tarde";
    private const string MsgIlegible = "No se pudo interpretar la respuesta del servicio de países";

    public record PaginaApi(List<Pais> Paises, bool HayMas);

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<RestCountriesOpciones> _opciones;

    public ConsumoApi(HttpClient http, IOptionsMonitor<RestCountriesOpciones> opciones)
    {
        _http = http;
        _opciones = opciones;
    }

    public async Task<List<Pais>> ObtenerTodosAsync(CancellationToken ct = default)
    {
        var todos = new List<Pais>();
        for (var pagina = 0; pagina < MaximoPaginas; pagina++)
        {
            var url = $"{Base}?limit={TamanoPagina}&offset={pagina * TamanoPagina}&{Campos}";
            var resultado = ConvertirPagina(await ConsultarAsync(url, ct));
            todos.AddRange(resultado.Paises);
            if (!resultado.HayMas)
            {
                break;
            }
        }
        return todos;
    }

    public async Task<List<Pais>> BuscarPorNombreAsync(string nombre, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionMonster("Escriba el nombre del país");
        }
        var url = $"{Base}/names.common/{Uri.EscapeDataString(nombre.Trim())}?{Campos}";
        return Convertir(await ConsultarAsync(url, ct));
    }

    public static List<Pais> Convertir(string json) => ConvertirPagina(json).Paises;

    public static PaginaApi ConvertirPagina(string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            var data = documento.RootElement.GetProperty("data");
            var paises = new List<Pais>();
            foreach (var objeto in data.GetProperty("objects").EnumerateArray())
            {
                paises.Add(APais(objeto));
            }
            return new PaginaApi(paises, HayMas(data));
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException
                                      or KeyNotFoundException or FormatException)
        {
            throw new ExcepcionMonster(MsgIlegible, e);
        }
    }

    private string Base => _opciones.CurrentValue.BaseUrl.TrimEnd('/');

    private static Pais APais(JsonElement objeto) => new()
    {
        Nombre = Recortar(Texto(objeto.GetProperty("names"), "common"))
                 ?? throw new InvalidOperationException("País sin nombre"),
        Capital = Recortar(PrimeraCapital(objeto)),
        Region = Recortar(Texto(objeto, "region")) ?? "",
        Subregion = Recortar(Texto(objeto, "subregion")),
        Poblacion = Poblacion(objeto),
        Area = Area(objeto),
        Moneda = Recortar(Monedas(objeto)),
    };

    private static string? PrimeraCapital(JsonElement objeto)
    {
        if (!objeto.TryGetProperty("capitals", out var capitales)
            || capitales.ValueKind != JsonValueKind.Array || capitales.GetArrayLength() == 0)
        {
            return null;
        }
        return Texto(capitales[0], "name");
    }

    private static string? Monedas(JsonElement objeto)
    {
        if (!objeto.TryGetProperty("currencies", out var monedas) || monedas.ValueKind != JsonValueKind.Array)
        {
            return null;
        }
        var nombres = monedas.EnumerateArray()
            .Select(m => Texto(m, "name") ?? Texto(m, "code"))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
        return nombres.Count == 0 ? null : string.Join(", ", nombres);
    }

    private static long Poblacion(JsonElement objeto)
    {
        if (!objeto.TryGetProperty("population", out var p) || p.ValueKind != JsonValueKind.Number)
        {
            return 0L;
        }
        return p.TryGetInt64(out var entero) ? entero : (long)p.GetDouble();
    }
    
    private static double? Area(JsonElement objeto)
    {
        if (!objeto.TryGetProperty("area", out var area))
        {
            return null;
        }
        if (area.ValueKind == JsonValueKind.Number)
        {
            return area.GetDouble();
        }
        if (area.ValueKind == JsonValueKind.Object
            && area.TryGetProperty("kilometers", out var km) && km.ValueKind == JsonValueKind.Number)
        {
            return km.GetDouble();
        }
        return null;
    }

    private static bool HayMas(JsonElement data) =>
        data.TryGetProperty("meta", out var meta) && meta.ValueKind == JsonValueKind.Object
        && meta.TryGetProperty("more", out var mas) && mas.ValueKind == JsonValueKind.True;

    private static string? Texto(JsonElement objeto, string campo) =>
        objeto.TryGetProperty(campo, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static string? Recortar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }
        var limpio = valor.Trim();
        return limpio.Length > Pais.LongitudMaxima ? limpio[..Pais.LongitudMaxima] : limpio;
    }

    private async Task<string> ConsultarAsync(string url, CancellationToken ct)
    {
        var clave = _opciones.CurrentValue.ApiKey;
        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new ExcepcionMonster("Falta configurar la API key");
        }

        using var peticion = new HttpRequestMessage(HttpMethod.Get, url);
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clave);
        peticion.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var respuesta = await _http.SendAsync(peticion, ct);
            return (int)respuesta.StatusCode switch
            {
                200 => await respuesta.Content.ReadAsStringAsync(ct),
                401 => throw new ExcepcionMonster("La API key no es válida"),
                403 or 429 => throw new ExcepcionMonster("Se superó el límite de consultas de la API, intente más tarde"),
                _ => throw new ExcepcionMonster(MsgIlegible),
            };
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ExcepcionMonster(MsgRed, e);
        }
    }
}
