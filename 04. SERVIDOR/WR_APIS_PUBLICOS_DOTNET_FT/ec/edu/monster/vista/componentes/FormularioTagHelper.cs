using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ec.edu.monster.vista.componentes;

[HtmlTargetElement("ui-formulario")]
public class FormularioTagHelper : TagHelper
{
    private readonly IUrlHelperFactory _urls;
    private readonly IHtmlGenerator _generador;

    public FormularioTagHelper(IUrlHelperFactory urls, IHtmlGenerator generador)
    {
        _urls = urls;
        _generador = generador;
    }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public string Accion { get; set; } = "";
    public string? Metodo { get; set; }
    public string? Clase { get; set; }
    public string? Confirmar { get; set; }
    public string? Progreso { get; set; }
    public bool Filtro { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var metodo = string.IsNullOrEmpty(Metodo) ? "post" : Metodo.ToLowerInvariant();
        output.TagName = "form";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("method", metodo);
        output.Attributes.SetAttribute("action", _urls.GetUrlHelper(ViewContext).Content("~" + Accion));
        if (!string.IsNullOrEmpty(Clase)) output.Attributes.SetAttribute("class", Clase);
        if (!string.IsNullOrEmpty(Confirmar)) output.Attributes.SetAttribute("data-confirmar", Confirmar);
        if (!string.IsNullOrEmpty(Progreso)) output.Attributes.SetAttribute("data-progreso", Progreso);
        if (Filtro) output.Attributes.SetAttribute("data-filtro", "");
        if (metodo == "post")
        {
            output.PreContent.AppendHtml(_generador.GenerateAntiforgery(ViewContext));
        }
    }
}
