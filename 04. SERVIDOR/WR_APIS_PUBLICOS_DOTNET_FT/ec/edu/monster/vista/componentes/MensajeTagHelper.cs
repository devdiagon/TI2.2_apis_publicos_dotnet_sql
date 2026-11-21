using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ec.edu.monster.vista.componentes;

[HtmlTargetElement("ui-mensaje")]
public class MensajeTagHelper : TagHelper
{
    public string? Tipo { get; set; }

    public string? Mensaje { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Mensaje))
        {
            output.SuppressOutput();
            return;
        }
        var tipo = Tipo is "error" or "exito" ? Tipo : "info";
        var icono = tipo switch { "error" => "warning-circle", "exito" => "check-circle", _ => "info" };

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"mensaje mensaje--{tipo}");
        output.Attributes.SetAttribute("role", tipo == "error" ? "alert" : "status");

        var i = new TagBuilder("i");
        i.AddCssClass("ph");
        i.AddCssClass("ph-" + icono);
        var texto = new TagBuilder("span");
        texto.InnerHtml.Append(Mensaje);
        output.Content.AppendHtml(i);
        output.Content.AppendHtml(texto);
    }
}
