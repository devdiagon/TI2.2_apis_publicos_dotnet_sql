using ec.edu.monster.modelo;
using ec.edu.monster.vista.modelos;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

[Route("reportes")]
public class ReporteController : ControladorBase
{
    private readonly PaisRepositorio _repositorio;

    public ReporteController(PaisRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var conteo = await _repositorio.ContarPorRegionAsync(ct);
        if (conteo.Count == 0)
        {
            return View("Reportes", new ReportesVista([], null, 0, null, []));
        }
        
        var mayor = conteo[0].Total;
        var promedios = await _repositorio.PromedioPoblacionPorRegionAsync(ct);
        var mayorPromedio = promedios.Count > 0 ? promedios.Max(p => p.Promedio) : 0;

        var vista = new ReportesVista(
            conteo.Select(c => new FilaConteo(Pais.EtiquetaRegion(c.Region), c.Total, c.Total * 100.0 / mayor)).ToList(),
            Pais.EtiquetaRegion(conteo[0].Region),
            mayor,
            await _repositorio.MasPobladoAsync(ct),
            promedios.Select(p => new FilaPromedio(
                Pais.EtiquetaRegion(p.Region), p.Promedio,
                mayorPromedio > 0 ? p.Promedio * 100.0 / mayorPromedio : 0)).ToList());
        return View("Reportes", vista);
    }
}
