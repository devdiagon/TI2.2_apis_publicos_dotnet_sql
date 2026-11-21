using ec.edu.monster.modelo;

namespace ec.edu.monster.vista.modelos;

public record ListaPaisesVista(IReadOnlyList<Pais> Paises, string? Region, string? Texto);
public record BuscarApiVista(string? Nombre, IReadOnlyList<Pais>? Resultados);

public class FormularioPais
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
    public string? Capital { get; set; }
    public string? Region { get; set; }
    public string? Subregion { get; set; }
    public string? Poblacion { get; set; }
    public string? Area { get; set; }
    public string? Moneda { get; set; }

    public bool Editando => !string.IsNullOrEmpty(Id);
}

public record UniversidadesVista(
    IReadOnlyList<Universidad> Guardadas,
    string? Pais,
    IReadOnlyList<Universidad>? Resultados,
    HashSet<string> Claves,
    string? ErrorBusqueda);

public class FormularioUniversidad
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
    public string? Dominio { get; set; }
    public string? SitioWeb { get; set; }
    public string? Pais { get; set; }

    public bool Editando => !string.IsNullOrEmpty(Id);
}

public record FilaConteo(string Region, int Total, double Porcentaje);
public record FilaPromedio(string Region, double Promedio, double Porcentaje);
public record ReportesVista(
    IReadOnlyList<FilaConteo> Conteo,
    string? RegionTop,
    int RegionTopTotal,
    Pais? MasPoblado,
    IReadOnlyList<FilaPromedio> Promedios);
