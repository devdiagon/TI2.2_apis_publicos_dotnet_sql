using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ec.edu.monster.excepciones;
using ec.edu.monster.modelo;
using ec.edu.monster.vista.modelos;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

[Route("paises")]
public partial class PaisController : ControladorBase
{
    private const string Pendiente = "PaisPendiente";

    private readonly PaisRepositorio _repositorio;
    private readonly ConsumoApi _api;

    public PaisController(PaisRepositorio repositorio, ConsumoApi api)
    {
        _repositorio = repositorio;
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Listar(string? region, string? texto, CancellationToken ct) =>
        View("Paises", new ListaPaisesVista(await _repositorio.ListarAsync(region, texto, ct), region, texto));

    [HttpGet("nuevo")]
    public IActionResult Nuevo() => Formulario(new FormularioPais());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var pais = await _repositorio.BuscarPorIdAsync(id, ct)
            ?? throw new ExcepcionMonster("El país no existe");
        return Formulario(new FormularioPais
        {
            Id = pais.Id.ToString(CultureInfo.InvariantCulture),
            Nombre = pais.Nombre,
            Capital = pais.Capital,
            Region = pais.Region,
            Subregion = pais.Subregion,
            Poblacion = pais.Poblacion.ToString(CultureInfo.InvariantCulture),
            Area = pais.Area?.ToString("0.##", CultureInfo.InvariantCulture),
            Moneda = pais.Moneda,
        });
    }

    [HttpGet("buscar-api")]
    public async Task<IActionResult> BuscarEnApi(string? nombre, CancellationToken ct)
    {
        IReadOnlyList<Pais>? resultados = null;
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            resultados = await _api.BuscarPorNombreAsync(nombre, ct);
        }
        return View("BuscarApi", new BuscarApiVista(nombre, resultados));
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> Guardar(string? id, string? nombre, string? capital, string? region,
        string? subregion, string? poblacion, string? area, string? moneda, CancellationToken ct)
    {
        var nuevo = string.IsNullOrWhiteSpace(id);
        if (!nuevo)
        {
            RetornoError = "/paises/" + Entero(id).ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            RetornoError = "/paises/nuevo";
        }

        var formulario = new FormularioPais
        {
            Id = nuevo ? null : id!.Trim(),
            Nombre = Limpio(nombre),
            Capital = Limpio(capital),
            Region = Limpio(region),
            Subregion = Limpio(subregion),
            Poblacion = Limpio(poblacion),
            Area = Limpio(area),
            Moneda = Limpio(moneda),
        };

        TempData[Pendiente] = JsonSerializer.Serialize(formulario);

        var pais = Validar(formulario);
        if (nuevo)
        {
            await _repositorio.GuardarAsync(pais, ct);
        }
        else
        {
            pais.Id = Entero(id);
            await _repositorio.ActualizarAsync(pais, ct);
        }

        TempData.Remove(Pendiente);
        Avisar("exito", nuevo ? "País guardado" : "País actualizado");
        return Redirigir("/paises");
    }

    [HttpPost("eliminar")]
    public async Task<IActionResult> Eliminar(string? id, CancellationToken ct)
    {
        await _repositorio.EliminarAsync(Entero(id), ct);
        Avisar("exito", "País eliminado");
        return Redirigir("/paises");
    }

    [HttpPost("importar")]
    public async Task<IActionResult> Importar(CancellationToken ct)
    {
        var total = await _repositorio.ImportarAsync(await _api.ObtenerTodosAsync(ct), ct);
        Avisar("exito", $"Importación finalizada: {total} países");
        return Redirigir("/paises");
    }

    private IActionResult Formulario(FormularioPais formulario)
    {
        if (TempData[Pendiente] is string json)
        {
            formulario = JsonSerializer.Deserialize<FormularioPais>(json) ?? formulario;
        }
        return View("PaisForm", formulario);
    }
    
    private static Pais Validar(FormularioPais f)
    {
        if (f.Nombre is null) throw new ExcepcionMonster("El nombre es obligatorio");
        if (f.Region is null) throw new ExcepcionMonster("La región es obligatoria");
        Longitud(f.Nombre, "El nombre");
        Longitud(f.Capital, "La capital");
        Longitud(f.Region, "La región");
        Longitud(f.Subregion, "La subregión");
        Longitud(f.Moneda, "La moneda");

        if (f.Poblacion is null || !DigitosRegex().IsMatch(f.Poblacion))
        {
            throw new ExcepcionMonster("La población debe ser un número entero mayor o igual a 0");
        }

        double? area = null;
        if (f.Area is not null)
        {
            if (!DecimalRegex().IsMatch(f.Area))
            {
                throw new ExcepcionMonster("El área debe ser un número mayor o igual a 0");
            }
            area = double.Parse(f.Area.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        return new Pais
        {
            Nombre = f.Nombre,
            Capital = f.Capital,
            Region = f.Region,
            Subregion = f.Subregion,
            Poblacion = long.Parse(f.Poblacion, CultureInfo.InvariantCulture),
            Area = area,
            Moneda = f.Moneda,
        };
    }

    private static void Longitud(string? valor, string campo)
    {
        if (valor is not null && valor.Length > Pais.LongitudMaxima)
        {
            throw new ExcepcionMonster($"{campo} no puede superar {Pais.LongitudMaxima} caracteres");
        }
    }

    private static string? Limpio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static int Entero(string? texto) =>
        int.TryParse(texto?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var n)
            ? n
            : throw new ExcepcionMonster("Identificador inválido");

    [GeneratedRegex(@"^\d{1,18}$")]
    private static partial Regex DigitosRegex();

    [GeneratedRegex(@"^\d{1,15}([.,]\d{1,6})?$")]
    private static partial Regex DecimalRegex();
}
