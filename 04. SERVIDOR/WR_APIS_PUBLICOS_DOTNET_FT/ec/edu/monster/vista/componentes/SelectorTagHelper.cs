using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ec.edu.monster.vista.componentes;

[HtmlTargetElement("ui-selector")]
public class SelectorTagHelper : TagHelper
{
    public string Id { get; set; } = "";
    public string? Nombre { get; set; }
    public string Etiqueta { get; set; } = "";
    public IEnumerable<KeyValuePair<string, string>> Opciones { get; set; } = [];
    public string? Seleccionada { get; set; }
    public string? Vacio { get; set; }
    public bool Conservar { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "bloque");

        var etiqueta = new TagBuilder("label");
        etiqueta.AddCssClass("etiqueta");
        etiqueta.Attributes["for"] = Id;
        etiqueta.InnerHtml.Append(Etiqueta);

        var selector = new TagBuilder("select");
        selector.AddCssClass("selector");
        selector.Attributes["id"] = Id;
        selector.Attributes["name"] = Nombre ?? Id;

        selector.InnerHtml.AppendHtml(Opcion("", string.IsNullOrEmpty(Vacio) ? "Seleccione…" : Vacio, false));
        var lista = Opciones.ToList();
        foreach (var (valor, texto) in lista)
        {
            selector.InnerHtml.AppendHtml(Opcion(valor, texto, valor == Seleccionada));
        }
        if (Conservar && !string.IsNullOrEmpty(Seleccionada) && lista.All(o => o.Key != Seleccionada))
        {
            selector.InnerHtml.AppendHtml(Opcion(Seleccionada, Seleccionada, true));
        }

        output.Content.AppendHtml(etiqueta);
        output.Content.AppendHtml(selector);
    }

    private static TagBuilder Opcion(string valor, string texto, bool seleccionada)
    {
        var o = new TagBuilder("option");
        o.Attributes["value"] = valor;
        if (seleccionada) o.Attributes["selected"] = "selected";
        o.InnerHtml.Append(texto);
        return o;
    }
}
