namespace ec.edu.monster.modelo;

public class Universidad
{
    public const int LongitudNombre = 200;
    public const int LongitudDominio = 100;
    public const int LongitudSitioWeb = 300;
    public const int LongitudPais = 100;

    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Dominio { get; set; }
    public string? SitioWeb { get; set; }
    public string Pais { get; set; } = "";

    public void CopiarDatosDe(Universidad origen)
    {
        Nombre = origen.Nombre;
        Dominio = origen.Dominio;
        SitioWeb = origen.SitioWeb;
        Pais = origen.Pais;
    }

    public static string Clave(string? nombre, string? pais) => $"{pais}\u001f{nombre}";
    
    public static bool EsUrlWeb(string? valor) =>
        Uri.TryCreate(valor, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}
