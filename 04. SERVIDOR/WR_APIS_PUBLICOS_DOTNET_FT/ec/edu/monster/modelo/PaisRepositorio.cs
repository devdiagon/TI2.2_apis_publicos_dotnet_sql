using ec.edu.monster.excepciones;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ec.edu.monster.modelo;

public record ConteoRegion(string Region, int Total);

public record PromedioRegion(string Region, double Promedio);

public class PaisRepositorio
{
    private const string MsgDuplicado = "Ya existe un país con ese nombre";
    private const string MsgNoExiste = "El país no existe";

    private readonly AppDbContext _db;

    public PaisRepositorio(AppDbContext db)
    {
        _db = db;
    }

    public async Task GuardarAsync(Pais pais, CancellationToken ct = default)
    {
        _db.Paises.Add(pais);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (EsDuplicado(e))
        {
            _db.Entry(pais).State = EntityState.Detached;
            throw new ExcepcionMonster(MsgDuplicado, e);
        }
    }

    public async Task ActualizarAsync(Pais pais, CancellationToken ct = default)
    {
        var existente = await _db.Paises.FindAsync([pais.Id], ct)
            ?? throw new ExcepcionMonster(MsgNoExiste);
        existente.CopiarDatosDe(pais);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (EsDuplicado(e))
        {
            throw new ExcepcionMonster(MsgDuplicado, e);
        }
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var filas = await _db.Paises.Where(p => p.Id == id).ExecuteDeleteAsync(ct);
        if (filas == 0)
        {
            throw new ExcepcionMonster(MsgNoExiste);
        }
    }

    public Task<Pais?> BuscarPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Paises.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    
    public async Task<List<Pais>> ListarAsync(string? region, string? texto, CancellationToken ct = default)
    {
        IQueryable<Pais> consulta = _db.Paises.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(region))
        {
            consulta = consulta.Where(p => p.Region == region);
        }
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(t) || (p.Capital != null && p.Capital.Contains(t)));
        }
        return await consulta.OrderBy(p => p.Nombre).ToListAsync(ct);
    }

    public async Task<int> ImportarAsync(IReadOnlyList<Pais> paises, CancellationToken ct = default)
    {
        var existentes = await _db.Paises.ToDictionaryAsync(p => p.Nombre, StringComparer.OrdinalIgnoreCase, ct);
        foreach (var pais in paises)
        {
            if (existentes.TryGetValue(pais.Nombre, out var actual))
            {
                actual.CopiarDatosDe(pais);
            }
            else
            {
                var nuevo = new Pais();
                nuevo.CopiarDatosDe(pais);
                _db.Paises.Add(nuevo);
                existentes[nuevo.Nombre] = nuevo;
            }
        }
        await _db.SaveChangesAsync(ct);
        return paises.Count;
    }

    public async Task<List<ConteoRegion>> ContarPorRegionAsync(CancellationToken ct = default) =>
        await _db.Paises.AsNoTracking()
            .GroupBy(p => p.Region)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Select(g => new ConteoRegion(g.Key, g.Count()))
            .ToListAsync(ct);

    public Task<Pais?> MasPobladoAsync(CancellationToken ct = default) =>
        _db.Paises.AsNoTracking().OrderByDescending(p => p.Poblacion).FirstOrDefaultAsync(ct);

    public async Task<List<PromedioRegion>> PromedioPoblacionPorRegionAsync(CancellationToken ct = default) =>
        await _db.Paises.AsNoTracking()
            .GroupBy(p => p.Region)
            .OrderByDescending(g => g.Average(p => (double)p.Poblacion))
            .Select(g => new PromedioRegion(g.Key, g.Average(p => (double)p.Poblacion)))
            .ToListAsync(ct);
    
    private static bool EsDuplicado(DbUpdateException e) =>
        e.InnerException is SqlException { Number: 2601 or 2627 };
}
