using FlipLeaf;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = ".static" });

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddFlipLeaf(builder.Configuration);
builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

var app = builder.Build();

#if DEBUG
app.UseDeveloperExceptionPage();
#endif

app.UseFlipLeaf(app.Environment);
app.UseStaticFiles();
app.UseAuthorization();
app.MapRazorPages();

app.Run();