using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;

namespace FlipLeaf
{
    public class FluidRenderer : IRenderingMiddleware
    {
        public void Render(RenderContext context, Action<RenderContext> next)
        {
            // Render fluid template
            TemplateContext templateContext = null;
            using (var reader = new StreamReader(context.Input, Encoding.UTF8, false, 1024, true))
            {
                if (ViewTemplate.TryParse(reader.ReadToEnd(), out var template))
                {
                    templateContext = new TemplateContext { MemberAccessStrategy = new IgnoreCaseMemberAccessStrategy() };
                    templateContext.MemberAccessStrategy.Register<SiteSettings>();
                    templateContext.SetValue("site", this._project.Settings);

                    if (context.Values.TryGetValue("PageData", out var page))
                    {
                        templateContext.SetValue("page", page);
                    }

                    var mem = new MemoryStream();

                    using (var writer = new StreamWriter(mem))
                    {
                        template.Render(writer, HtmlEncoder.Default, templateContext);
                    }

                    mem.Position = 0;
                    context.Output = mem;
                }
            }

            // Pass to next pipeline stage
            next?.Invoke(context);

            // Include the containing layout if necessary
            var layoutFile = templateContext?.GetValue("page").GetValue("layout", templateContext).ToStringValue();
            if (layoutFile != null)
            {
                if (Path.GetExtension(layoutFile) == "")
                {
                    layoutFile += ".html";
                }

                // read source from input
                string source;
                context.Output.Position = 0;
                using (var reader = new StreamReader(context.Output))
                {
                    source = reader.ReadToEnd();
                }

                var layoutText = File.ReadAllText(Path.Combine(this._project.Path, this._project.Settings.LayoutFolder, layoutFile));
                templateContext.AmbientValues.Add("Body", source);
                if (!ViewTemplate.TryParse(layoutText, out var layoutTemplate))
                {
                    throw new ParseException();
                }

                layoutTemplate.Render(templateContext);
            }
        }

        class IgnoreCaseMemberAccessStrategy : IMemberAccessStrategy
        {

            private Dictionary<string, IMemberAccessor> _map = new Dictionary<string, IMemberAccessor>(StringComparer.OrdinalIgnoreCase);

            public object Get(object obj, string name)
            {
                // Look for specific property map
                if (_map.TryGetValue(Key(obj.GetType(), name), out var getter))
                {
                    return getter.Get(obj, name);
                }

                // Look for a catch-all getter
                if (_map.TryGetValue(Key(obj.GetType(), "*"), out getter))
                {
                    return getter.Get(obj, name);
                }

                return null;
            }

            public void Register(Type type, string name, IMemberAccessor getter)
            {
                _map[Key(type, name)] = getter;
            }

            private string Key(Type type, string name) => $"{type.Name}.{name}";
        }

        class ViewTemplate : BaseFluidTemplate<ViewTemplate>
        {
            static ViewTemplate()
            {
                Factory.RegisterTag<RenderBodyTag>("renderbody");
            }
        }

        public class RenderBodyTag : SimpleTag
        {
            public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
            {
                if (context.AmbientValues.TryGetValue("Body", out var body))
                {
                    await writer.WriteAsync((string)body);
                }
                else
                {
                    throw new ParseException("Could not render body, Layouts can't be evaluated directly.");
                }

                return Completion.Normal;
            }
        }
    }

}