using ec.edu.monster.excepciones;
using ec.edu.monster.modelo;
using Microsoft.Extensions.Options;
using Xunit;

namespace WR_APIS_PUBLICOS_DOTNET_FT.ec.edu.monster.pruebas;

public class ConsumoApiTests
{
    private static string Respuesta(string objetos, bool mas) =>
        "{\"data\":{\"objects\":[" + objetos + "],\"meta\":{\"total\":1,\"more\":" + (mas ? "true" : "false") + "}}}";

    private const string Ecuador = """
        {"names":{"common":"Ecuador"},
         "capitals":[{"attributes":{"primary":true},"name":"Quito"}],
         "region":"Americas","subregion":"South America",
         "area":{"kilometers":276841,"miles":106888.9},
         "currencies":[{"code":"USD","name":"United States dollar","symbol":"$"}],
         "population":17500000,"_meta":{"lastUpdatedTimestamp":1788724017}}
        """;

    private sealed class OpcionesFijas : IOptionsMonitor<RestCountriesOpciones>
    {
        public RestCountriesOpciones CurrentValue { get; } = new();
        public RestCountriesOpciones Get(string? nombre) => CurrentValue;
        public IDisposable? OnChange(Action<RestCountriesOpciones, string?> escucha) => null;
    }

    [Fact]
    public void ConvierteUnPaisCompleto()
    {
        var paises = ConsumoApi.Convertir(Respuesta(Ecuador, false));

        var p = Assert.Single(paises);
        Assert.Equal("Ecuador", p.Nombre);
        Assert.Equal("Quito", p.Capital);
        Assert.Equal("Americas", p.Region);
        Assert.Equal("South America", p.Subregion);
        Assert.Equal(17500000L, p.Poblacion);
        Assert.Equal(276841d, p.Area);
        Assert.Equal("United States dollar", p.Moneda);
    }

    [Fact]
    public void ElAreaSeTomaEnKilometros()
    {
        var json = """{"names":{"common":"X"},"region":"Asia","area":{"kilometers":106888.9,"miles":1}}""";
        Assert.Equal(106888.9, ConsumoApi.Convertir(Respuesta(json, false))[0].Area);
    }

    [Fact]
    public void PaisSinCapitalNiMonedaQuedaNulo()
    {
        var json = """{"names":{"common":"Antarctica"},"capitals":[],"region":"Antarctic","currencies":[],"population":0}""";
        var p = ConsumoApi.Convertir(Respuesta(json, false))[0];
        Assert.Null(p.Capital);
        Assert.Null(p.Moneda);
        Assert.Equal("Antarctic", p.Region);
    }

    [Fact]
    public void CampoAusenteSeTrataComoSinDato()
    {
        var p = ConsumoApi.Convertir(Respuesta("""{"names":{"common":"X"},"region":"Asia"}""", false))[0];
        Assert.Null(p.Capital);
        Assert.Null(p.Subregion);
        Assert.Null(p.Area);
        Assert.Null(p.Moneda);
        Assert.Equal(0L, p.Poblacion);
    }

    [Fact]
    public void VariasMonedasSeUnenConComa()
    {
        var json = """
            {"names":{"common":"Panama"},"region":"Americas","population":4000000,
             "currencies":[{"code":"PAB","name":"Panamanian balboa"},{"code":"USD","name":"United States dollar"}]}
            """;
        Assert.Equal("Panamanian balboa, United States dollar", ConsumoApi.Convertir(Respuesta(json, false))[0].Moneda);
    }

    [Fact]
    public void RecortaTextosLargosA100Caracteres()
    {
        var json = "{\"names\":{\"common\":\"" + new string('A', 150) + "\"},\"region\":\"Asia\"}";
        Assert.Equal(100, ConsumoApi.Convertir(Respuesta(json, false))[0].Nombre.Length);
    }

    [Fact]
    public void ListaVaciaNoEsError() => Assert.Empty(ConsumoApi.Convertir(Respuesta("", false)));

    [Fact]
    public void IndicaSiHayMasPaginas()
    {
        Assert.True(ConsumoApi.ConvertirPagina(Respuesta(Ecuador, true)).HayMas);
        Assert.False(ConsumoApi.ConvertirPagina(Respuesta(Ecuador, false)).HayMas);
    }

    [Theory]
    [InlineData("esto no es json")]
    [InlineData("{\"errors\":[{\"code\":\"x.y\"}]}")]
    [InlineData("")]
    public void JsonInvalidoLanzaExcepcionMonster(string json) =>
        Assert.Throws<ExcepcionMonster>(() => ConsumoApi.Convertir(json));

    [Fact]
    public async Task BuscarConNombreVacioLanzaExcepcionMonster()
    {
        var api = new ConsumoApi(new HttpClient(), new OpcionesFijas());
        await Assert.ThrowsAsync<ExcepcionMonster>(() => api.BuscarPorNombreAsync("  "));
    }

    [Fact]
    public async Task SinApiKeyLanzaExcepcionMonster()
    {
        var api = new ConsumoApi(new HttpClient(), new OpcionesFijas());
        var error = await Assert.ThrowsAsync<ExcepcionMonster>(() => api.BuscarPorNombreAsync("Ecuador"));
        Assert.Equal("Falta configurar la API key", error.Message);
    }
}
