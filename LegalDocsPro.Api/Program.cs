using LegalDocsPro.Domain.Interfaces;
using LegalDocsPro.Infrastructure.Persistence.Contexts;
using LegalDocsPro.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Entity Framework Core con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// 2. Inyección de Dependencias: Repositorios
// Usamos AddScoped para que se cree una instancia por cada petición HTTP
builder.Services.AddScoped<IContractRepository, ContractRepository>();

// Registrar MediatR escaneando el ensamblado de la capa de Aplicación
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LegalDocsPro.Application.Features.Contracts.Commands.CreateContractCommand).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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