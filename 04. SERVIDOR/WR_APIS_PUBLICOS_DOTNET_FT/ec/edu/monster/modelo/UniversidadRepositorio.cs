using ec.edu.monster.excepciones;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ec.edu.monster.modelo;

public class UniversidadRepositorio
{
    private const string MsgDuplicado = "Esa universidad ya está guardada";
    private const string MsgNoExiste = "La universidad no existe";

    private readonly AppDbContext _db;

    public UniversidadRepositorio(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Universidad>> ListarAsync(CancellationToken ct = default) =>
        _db.Universidades.AsNoTracking().OrderBy(u => u.Pais).ThenBy(u => u.Nombre).ToListAsync(ct);

    public Task<Universidad?> BuscarPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Universidades.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task GuardarAsync(Universidad universidad, CancellationToken ct = default)
    {
        _db.Universidades.Add(universidad);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (EsDuplicado(e))
        {
            _db.Entry(universidad).State = EntityState.Detached;
            throw new ExcepcionMonster(MsgDuplicado, e);
        }
    }

    public async Task ActualizarAsync(Universidad universidad, CancellationToken ct = default)
    {
        var existente = await _db.Universidades.FindAsync([universidad.Id], ct)
            ?? throw new ExcepcionMonster(MsgNoExiste);
        existente.CopiarDatosDe(universidad);
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
        var filas = await _db.Universidades.Where(u => u.Id == id).ExecuteDeleteAsync(ct);
        if (filas == 0)
        {
            throw new ExcepcionMonster(MsgNoExiste);
        }
    }

    public async Task<HashSet<string>> ClavesGuardadasAsync(IEnumerable<Universidad> resultados, CancellationToken ct = default)
    {
        var paises = resultados.Select(r => r.Pais).Distinct().ToList();
        var guardadas = await _db.Universidades.AsNoTracking()
            .Where(u => paises.Contains(u.Pais))
            .Select(u => new { u.Nombre, u.Pais })
            .ToListAsync(ct);
        return guardadas.Select(u => Universidad.Clave(u.Nombre, u.Pais)).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    
    private static bool EsDuplicado(DbUpdateException e) =>
        e.InnerException is SqlException { Number: 2601 or 2627 };
}
