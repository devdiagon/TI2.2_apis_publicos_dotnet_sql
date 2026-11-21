using ec.edu.monster.excepciones;
using ec.edu.monster.modelo;
using Microsoft.Extensions.Options;
using Xunit;

namespace WR_APIS_PUBLICOS_DOTNET_FT.ec.edu.monster.pruebas;

public class ConsumoApiUniversidadesTests
{
    private const string Epn = """
        {"web_pages":["http://www.epn.edu.ec/"],"country":"Ecuador","state-province":null,
         "alpha_two_code":"EC","name":"Escuela Politécnica Nacional","domains":["epn.edu.ec"]}
        """;

    private sealed class OpcionesFijas : IOptionsMonitor<UniversidadesOpciones>
    {
        public UniversidadesOpciones CurrentValue { get; } = new();
        public UniversidadesOpciones Get(string? nombre) => CurrentValue;
        public IDisposable? OnChange(Action<UniversidadesOpciones, string?> escucha) => null;
    }

    [Fact]
    public void ConvierteUnaUniversidadCompleta()
    {
        var u = Assert.Single(ConsumoApiUniversidades.Convertir("[" + Epn + "]"));
        Assert.Equal("Escuela Politécnica Nacional", u.Nombre);
        Assert.Equal("epn.edu.ec", u.Dominio);
        Assert.Equal("http://www.epn.edu.ec/", u.SitioWeb);
        Assert.Equal("Ecuador", u.Pais);
    }

    [Fact]
    public void SinDominiosNiPaginasQuedanNulos()
    {
        var json = """[{"name":"X","country":"Ecuador","domains":[],"web_pages":[]}]""";
        var u = Assert.Single(ConsumoApiUniversidades.Convertir(json));
        Assert.Null(u.Dominio);
        Assert.Null(u.SitioWeb);

        var sinCampos = Assert.Single(ConsumoApiUniversidades.Convertir("""[{"name":"Y","country":"Ecuador"}]"""));
        Assert.Null(sinCampos.Dominio);
        Assert.Null(sinCampos.SitioWeb);
    }

    [Fact]
    public void SiHayVariosSeTomaElPrimero()
    {
        var json = """[{"name":"X","country":"Peru","domains":["a.pe","b.pe"],"web_pages":["http://a.pe","http://b.pe"]}]""";
        var u = ConsumoApiUniversidades.Convertir(json)[0];
        Assert.Equal("a.pe", u.Dominio);
        Assert.Equal("http://a.pe", u.SitioWeb);
    }

    [Fact]
    public void ListaVaciaNoEsError() => Assert.Empty(ConsumoApiUniversidades.Convertir("[]"));

    [Fact]
    public void RecortaTextosLargos()
    {
        var json = "[{\"name\":\"" + new string('A', 250) + "\",\"country\":\"Ecuador\"}]";
        Assert.Equal(Universidad.LongitudNombre, ConsumoApiUniversidades.Convertir(json)[0].Nombre.Length);
    }

    [Theory]
    [InlineData("esto no es json")]
    [InlineData("{\"error\":\"x\"}")]
    [InlineData("")]
    [InlineData("""[{"country":"Ecuador"}]""")]
    [InlineData("""[{"name":"Sin país"}]""")]
    public void JsonInvalidoLanzaExcepcionMonster(string json) =>
        Assert.Throws<ExcepcionMonster>(() => ConsumoApiUniversidades.Convertir(json));

    [Fact]
    public async Task BuscarConPaisVacioLanzaExcepcionMonster()
    {
        var api = new ConsumoApiUniversidades(new HttpClient(), new OpcionesFijas());
        await Assert.ThrowsAsync<ExcepcionMonster>(() => api.BuscarPorPaisAsync("  "));
    }

    [Theory]
    [InlineData("http://www.epn.edu.ec/", true)]
    [InlineData("https://espol.edu.ec", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:text/html,<b>x</b>", false)]
    [InlineData("www.sin-esquema.edu.ec", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void SoloLasUrlHttpSeMuestranComoEnlace(string? valor, bool esperado) =>
        Assert.Equal(esperado, Universidad.EsUrlWeb(valor));

    [Fact]
    public void LaClaveNoDistingueMayusculasAlCompararse()
    {
        var claves = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Universidad.Clave("Universidad Casa Grande", "Ecuador") };
        Assert.Contains(Universidad.Clave("universidad casa grande", "ecuador"), claves);
        Assert.DoesNotContain(Universidad.Clave("Universidad Casa Grande", "Peru"), claves);
    }
}
