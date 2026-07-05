using System.Text;
using Auth.Domain.Ports;
using Auth.Infrastructure;
using Auth.Infrastructure.Security;
using Auth.Application;
using Catalogo.Application;
using Catalogo.Infrastructure;
using Estudiantes.Application;
using Estudiantes.Infrastructure;
using Inscripciones.Application;
using Inscripciones.Domain.Ports;
using Inscripciones.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Persistence.Infrastructure;
using Programas.Application;
using Programas.Infrastructure;
using RegistroMs.Api.Adapters;
using RegistroMs.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDatabasePersistence(builder.Configuration);

builder.Services.AddEstudiantesApplication();
builder.Services.AddEstudiantesInfrastructure(builder.Configuration);
builder.Services.AddProgramasApplication();
builder.Services.AddProgramasInfrastructure(builder.Configuration);
builder.Services.AddCatalogoApplication();
builder.Services.AddCatalogoInfrastructure(builder.Configuration);
builder.Services.AddInscripcionesApplication();
builder.Services.AddInscripcionesInfrastructure(builder.Configuration);
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure(builder.Configuration);

builder.Services.AddScoped<IProgramasClient, ProgramasModuleAdapter>();
builder.Services.AddScoped<ICatalogoClient, CatalogoModuleAdapter>();
builder.Services.AddScoped<IEstudiantesClient, EstudiantesModuleAdapter>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtKey = jwtSection["Key"] ?? "RegistroMs-Dev-Secret-Key-Min-32-Chars!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "registro-ms",
            ValidAudience = jwtSection["Audience"] ?? "registro-app",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Microfrontends", policy =>
        policy.WithOrigins("http://localhost:5100", "http://localhost:5101", "http://localhost:5102")
            .AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<DatabaseExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Microfrontends");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SessionActivityMiddleware>();
app.MapControllers();
app.Run();
