using Orquestador.Shell;
using Orquestador.Shell.Options;
using Orquestador.Shell.Services;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.Configure<MicrofrontendOptions>(
    builder.Configuration.GetSection(MicrofrontendOptions.SectionName));
builder.Services.AddScoped<ShellAuthService>();
builder.Services.AddHostedService<MicrofrontendProcessHost>();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapRazorPages();
app.MapGet("/login", () => Results.Redirect("/"));
app.MapReverseProxy();
app.MapFallbackToPage("/_Host");

app.Run();
