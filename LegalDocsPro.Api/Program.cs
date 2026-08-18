using FluentValidation;
using LegalDocsPro.Api.Middlewares;
using LegalDocsPro.Application.Common.Behaviours;
using LegalDocsPro.Domain.Interfaces;
using LegalDocsPro.Infrastructure.Persistence.Contexts;
using LegalDocsPro.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Infrastructure.Authentication;
using System.Text;
using Microsoft.OpenApi.Models;
using LegalDocsPro.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Entity Framework Core con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// 2. Inyección de Dependencias: Repositorios
builder.Services.AddScoped<IContractRepository, ContractRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Permite acceder al contexto HTTP (Request, Headers, etc.)
builder.Services.AddHttpContextAccessor();

// Registra nuestro servicio
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. Configurar MediatR y FluentValidation (NUEVO)
var applicationAssembly = typeof(LegalDocsPro.Application.Features.Contracts.Commands.CreateContractCommand).Assembly;

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(applicationAssembly);
    // Agregamos el guardia de seguridad al pipeline de MediatR
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
});

// Registramos todos los validadores que existan en la capa de Aplicación
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// --- CONFIGURACIÓN DE JWT ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});
// ----------------------------


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LegalDocsPro API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorización JWT usando el esquema Bearer. Escribe 'Bearer' [espacio] y luego tu token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// 👇 AGREGA ESTA CONFIGURACIÓN DE CORS 👇
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", builder =>
    {
        builder.AllowAnyOrigin()    // Permite conexiones desde cualquier dominio local (luego en prod se restringe)
               .AllowAnyHeader()    // Permite enviar cualquier tipo de dato (incluyendo tokens de autorización)
               .AllowAnyMethod();   // Permite usar GET, POST, PUT, PATCH, DELETE
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// 👇 AGREGA ESTO AQUÍ (Preferiblemente bien arriba) 👇
app.UseExceptionHandler(opt => { });



//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles(); // Permite a la API servir archivos físicos como PDFs o imágenes

app.UseHttpsRedirection();

// 👇 ACTIVA CORS AQUÍ (Antes de la Autenticación) 👇
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

// Redirigir la ruta raíz a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();
app.Run();