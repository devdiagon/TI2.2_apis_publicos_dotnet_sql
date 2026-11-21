using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ec.edu.monster.vista.componentes;

[HtmlTargetElement("ui-campo")]
public class CampoTagHelper : TagHelper
{
    public string Id { get; set; } = "";
    public string? Nombre { get; set; }
    public string Etiqueta { get; set; } = "";
    public string? Valor { get; set; }
    public string? Tipo { get; set; }
    public string? Placeholder { get; set; }
    public string? Maxlength { get; set; }
    public string? Inputmode { get; set; }
    public string? Autocomplete { get; set; }
    public bool Requerido { get; set; }
    public bool SoloNumeros { get; set; }
    public bool SoloDecimal { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "bloque");

        var etiqueta = new TagBuilder("label");
        etiqueta.AddCssClass("etiqueta");
        etiqueta.Attributes["for"] = Id;
        etiqueta.InnerHtml.Append(Etiqueta);

        var campo = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        campo.AddCssClass("campo");
        campo.Attributes["id"] = Id;
        campo.Attributes["name"] = Nombre ?? Id;
        campo.Attributes["type"] = Tipo ?? "text";
        campo.Attributes["value"] = Valor ?? "";
        if (!string.IsNullOrEmpty(Placeholder)) campo.Attributes["placeholder"] = Placeholder;
        if (!string.IsNullOrEmpty(Maxlength)) campo.Attributes["maxlength"] = Maxlength;
        if (!string.IsNullOrEmpty(Inputmode)) campo.Attributes["inputmode"] = Inputmode;
        if (!string.IsNullOrEmpty(Autocomplete)) campo.Attributes["autocomplete"] = Autocomplete;
        if (SoloNumeros)
        {
            campo.Attributes["data-solo-numeros"] = "";
            campo.Attributes["pattern"] = "[0-9]*";
            campo.Attributes["title"] = "Solo dígitos";
        }
        if (SoloDecimal)
        {
            campo.Attributes["data-solo-decimal"] = "";
            campo.Attributes["pattern"] = "[0-9]+([.,][0-9]+)?";
            campo.Attributes["title"] = "Solo dígitos y un separador decimal";
        }
        if (Requerido) campo.Attributes["required"] = "required";

        output.Content.AppendHtml(etiqueta);
        output.Content.AppendHtml(campo);
    }
}
