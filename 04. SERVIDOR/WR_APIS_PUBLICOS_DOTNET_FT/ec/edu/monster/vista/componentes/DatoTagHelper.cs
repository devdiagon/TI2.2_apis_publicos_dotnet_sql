using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ec.edu.monster.vista.componentes;

[HtmlTargetElement("ui-dato")]
public class DatoTagHelper : TagHelper
{
    public string? Valor { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Valor))
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", "sin-dato");
            output.Content.SetContent("—");
        }
        else
        {
            output.TagName = null;
            output.Content.SetContent(Valor);
        }
    }
}
