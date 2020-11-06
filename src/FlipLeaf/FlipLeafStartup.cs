using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlipLeaf
{
    /// <summary>
    /// Contains default startup class for a FlipLeaf app.
    /// </summary>
    /// <remarks>
    /// You can inherit from this class and register your custom services by overriding the <see cref="ConfigureServices(IServiceCollection)"/> method.
    /// </remarks>
    public class FlipLeafStartup
    {
        private readonly IConfiguration _config;

        public FlipLeafStartup(IConfiguration config)
        {
            _config = config;
        }

        protected IConfiguration Config => _config;

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddFlipLeaf(_config);
            services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if DEBUG
            app.UseDeveloperExceptionPage();
#endif
            app.UseFlipLeaf(env);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapRazorPages());
        }
    }
}
