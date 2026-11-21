namespace ec.edu.monster.modelo;

public class Pais
{
    public const int LongitudMaxima = 100;

    public static readonly IReadOnlyList<KeyValuePair<string, string>> Regiones =
    [
        new("Americas", "América"),
        new("Europe", "Europa"),
        new("Asia", "Asia"),
        new("Africa", "África"),
        new("Oceania", "Oceanía"),
        new("Antarctic", "Antártida"),
    ];

    public static string EtiquetaRegion(string? region) =>
        Regiones.FirstOrDefault(r => r.Key == region).Value ?? region ?? "";

    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Capital { get; set; }
    public string Region { get; set; } = "";
    public string? Subregion { get; set; }
    public long Poblacion { get; set; }
    public double? Area { get; set; }
    public string? Moneda { get; set; }
    
    public void CopiarDatosDe(Pais origen)
    {
        Nombre = origen.Nombre;
        Capital = origen.Capital;
        Region = origen.Region;
        Subregion = origen.Subregion;
        Poblacion = origen.Poblacion;
        Area = origen.Area;
        Moneda = origen.Moneda;
    }
}
