using FluentValidation;
using LegalDocsPro.Api.Middlewares;
using LegalDocsPro.Application.Common.Behaviours;
using LegalDocsPro.Domain.Interfaces;
using LegalDocsPro.Infrastructure.Persistence.Contexts;
using LegalDocsPro.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Entity Framework Core con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// 2. Inyección de Dependencias: Repositorios
builder.Services.AddScoped<IContractRepository, ContractRepository>();

// 3. Configurar MediatR y FluentValidation (NUEVO)
var applicationAssembly = typeof(LegalDocsPro.Application.Features.Contracts.Commands.CreateContractCommand).Assembly;

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(applicationAssembly);
    // Agregamos el guardia de seguridad al pipeline de MediatR
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
});

// Registramos todos los validadores que existan en la capa de Aplicación
builder.Services.AddValidatorsFromAssembly(applicationAssembly);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Activar nuestro Middleware atrapador de errores (NUEVO)
// Debe ir al principio para que atrape los errores de todo lo que sigue abajo
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Redirigir la ruta raíz a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();
app.Run();