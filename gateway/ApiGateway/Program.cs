var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Microfrontends", policy =>
        policy.WithOrigins("http://localhost:5100", "http://localhost:5101", "http://localhost:5102")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("Microfrontends");
app.MapReverseProxy();
app.Run();
