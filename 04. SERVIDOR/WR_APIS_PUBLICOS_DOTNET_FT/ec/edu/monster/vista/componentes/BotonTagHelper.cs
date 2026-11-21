using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ec.edu.monster.vista.componentes;

[HtmlTargetElement("ui-boton")]
public class BotonTagHelper : TagHelper
{
    private readonly IUrlHelperFactory _urls;

    public BotonTagHelper(IUrlHelperFactory urls)
    {
        _urls = urls;
    }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public string? Tipo { get; set; }

    public string? Href { get; set; }

    public string? Icono { get; set; }

    public bool Completo { get; set; }

    public bool Salida { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var clases = "boton boton--" + (string.IsNullOrEmpty(Tipo) ? "primario" : Tipo)
                     + (Completo ? " boton--completo" : "") + (Salida ? " salida" : "");
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", clases);
        if (!string.IsNullOrEmpty(Href))
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", _urls.GetUrlHelper(ViewContext).Content("~" + Href));
        }
        else
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "submit");
        }
        if (!string.IsNullOrEmpty(Icono))
        {
            var icono = new TagBuilder("i");
            icono.AddCssClass("ph");
            icono.AddCssClass("ph-" + Icono);
            output.PreContent.AppendHtml(icono);
        }
    }
}
