using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;

namespace FlipLeaf
{
    partial class Engine
    {
        private static class Fluid
        {
            public static bool ParsePage(ref string source, object pageContext, SiteSettings site, out TemplateContext context)
            {
                context = null;

                if (!ViewTemplate.TryParse(source, out var template))
                {
                    return false;
                }

                context = new TemplateContext { MemberAccessStrategy = new IgnoreCaseMemberAccessStrategy() };
                context.MemberAccessStrategy.Register<SiteSettings>();
                context.SetValue("page", pageContext);
                context.SetValue("site", site);

                source = template.Render(context);

                return true;
            }

            public static bool ApplyLayout(ref string source, TemplateContext context, string root)
            {
                var layoutFile = context.GetValue("page").GetValue("layout", context).ToStringValue();
                if (layoutFile != null)
                {
                    if (Path.GetExtension(layoutFile) == "")
                        layoutFile += ".html";

                    var layoutText = File.ReadAllText(Path.Combine(root, DefaultLayoutsFolder, layoutFile));
                    context.AmbientValues.Add("Body", source);
                    if (!ViewTemplate.TryParse(layoutText, out var layoutTemplate))
                    {
                        return false;
                    }

                    source = layoutTemplate.Render(context);
                }

                return true;
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
}