using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlipLeaf.TagHelpers
{
    public class ManageBreadcrumbTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ManageBreadcrumbTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        /// <summary>
        /// Gets or sets the <see cref="Markup.ViewContext"/> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        public string Home { get; set; } = "(Home)";

        public string Path { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext == null)
            {
                return;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            output.TagName = "ol";
            output.Attributes.Add("class", "breadcrumb");

            output.Content.AppendHtml($"<li class=\"breadcrumb-item\"><a href=\"{urlHelper.Page("Browse", new { path = string.Empty })}\">{Home}</a></li>");

            var parts = this.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                var item = parts[i];
                if (i < parts.Length - 1)
                {
                    var url = urlHelper.Page("Browse", new { path = string.Join('/', parts, 0, i + 1) });
                    output.Content.AppendHtml($"<li class=\"breadcrumb-item\"><a href=\"{url}\">{item}</a></li>");
                }
                else
                {
                    output.Content.AppendHtml($"<li class=\"breadcrumb-item active\" aria-current=\"page\">{item}</li>");
                }
            }
        }
    }
}
