using System.Globalization;
using System.Text.Json;
using ec.edu.monster.excepciones;
using ec.edu.monster.modelo;
using ec.edu.monster.vista.modelos;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

[Route("universidades")]
public class UniversidadController : ControladorBase
{
    private const string Pendiente = "UniversidadPendiente";

    private readonly UniversidadRepositorio _repositorio;
    private readonly ConsumoApiUniversidades _api;

    public UniversidadController(UniversidadRepositorio repositorio, ConsumoApiUniversidades api)
    {
        _repositorio = repositorio;
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? pais, CancellationToken ct)
    {
        var guardadas = await _repositorio.ListarAsync(ct);
        IReadOnlyList<Universidad>? resultados = null;
        var yaGuardadas = new HashSet<string>();
        string? errorBusqueda = null;

        if (!string.IsNullOrWhiteSpace(pais))
        {
            try
            {
                var encontradas = await _api.BuscarPorPaisAsync(pais, ct);
                resultados = encontradas;
                yaGuardadas = await _repositorio.ClavesGuardadasAsync(encontradas, ct);
            }
            catch (ExcepcionMonster e)
            {
                errorBusqueda = e.Message;
            }
        }
        return View("Universidades", new UniversidadesVista(guardadas, pais, resultados, yaGuardadas, errorBusqueda));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var u = await _repositorio.BuscarPorIdAsync(id, ct)
            ?? throw new ExcepcionMonster("La universidad no existe");
        var formulario = new FormularioUniversidad
        {
            Id = u.Id.ToString(CultureInfo.InvariantCulture),
            Nombre = u.Nombre,
            Dominio = u.Dominio,
            SitioWeb = u.SitioWeb,
            Pais = u.Pais,
        };
        if (TempData[Pendiente] is string json)
        {
            formulario = JsonSerializer.Deserialize<FormularioUniversidad>(json) ?? formulario;
        }
        return View("UniversidadForm", formulario);
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> Guardar(string? id, string? nombre, string? dominio, string? sitioWeb,
        string? pais, string? busqueda, CancellationToken ct)
    {
        RetornoError = "/universidades";
        var nuevo = string.IsNullOrWhiteSpace(id);
        var listaDeResultados = RutaLista(busqueda);
        RetornoError = nuevo ? listaDeResultados : "/universidades/" + Entero(id).ToString(CultureInfo.InvariantCulture);

        var formulario = new FormularioUniversidad
        {
            Id = nuevo ? null : id!.Trim(),
            Nombre = Limpio(nombre),
            Dominio = Limpio(dominio),
            SitioWeb = Limpio(sitioWeb),
            Pais = Limpio(pais),
        };
        if (!nuevo)
        {
            TempData[Pendiente] = JsonSerializer.Serialize(formulario);
        }

        var universidad = Validar(formulario);
        if (nuevo)
        {
            await _repositorio.GuardarAsync(universidad, ct);
            Avisar("exito", "Universidad guardada");
            return Redirigir(listaDeResultados);
        }

        universidad.Id = Entero(id);
        await _repositorio.ActualizarAsync(universidad, ct);
        TempData.Remove(Pendiente);
        Avisar("exito", "Universidad actualizada");
        return Redirigir("/universidades");
    }

    [HttpPost("eliminar")]
    public async Task<IActionResult> Eliminar(string? id, CancellationToken ct)
    {
        RetornoError = "/universidades";
        await _repositorio.EliminarAsync(Entero(id), ct);
        Avisar("exito", "Universidad eliminada");
        return Redirigir("/universidades");
    }

    private static string RutaLista(string? busqueda) =>
        string.IsNullOrWhiteSpace(busqueda)
            ? "/universidades"
            : "/universidades?pais=" + Uri.EscapeDataString(busqueda.Trim());
    
    private static Universidad Validar(FormularioUniversidad f)
    {
        if (f.Nombre is null) throw new ExcepcionMonster("El nombre es obligatorio");
        if (f.Pais is null) throw new ExcepcionMonster("El país es obligatorio");
        Longitud(f.Nombre, "El nombre", Universidad.LongitudNombre);
        Longitud(f.Pais, "El país", Universidad.LongitudPais);
        Longitud(f.Dominio, "El dominio", Universidad.LongitudDominio);
        Longitud(f.SitioWeb, "El sitio web", Universidad.LongitudSitioWeb);
        if (f.SitioWeb is not null && !Universidad.EsUrlWeb(f.SitioWeb))
        {
            throw new ExcepcionMonster("El sitio web debe ser una dirección que empiece por http:// o https://");
        }
        return new Universidad
        {
            Nombre = f.Nombre,
            Dominio = f.Dominio,
            SitioWeb = f.SitioWeb,
            Pais = f.Pais,
        };
    }

    private static void Longitud(string? valor, string campo, int maximo)
    {
        if (valor is not null && valor.Length > maximo)
        {
            throw new ExcepcionMonster($"{campo} no puede superar {maximo} caracteres");
        }
    }

    private static string? Limpio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static int Entero(string? texto) =>
        int.TryParse(texto?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var n)
            ? n
            : throw new ExcepcionMonster("Identificador inválido");
}
