using FluentValidation;
using LegalDocsPro.Api.Middlewares;
using LegalDocsPro.Api.Services;
using LegalDocsPro.Application.Common.Behaviours;
using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Interfaces;
using LegalDocsPro.Infrastructure.Authentication;
using LegalDocsPro.Infrastructure.Events;
using LegalDocsPro.Infrastructure.Persistence;
using LegalDocsPro.Infrastructure.Persistence.Contexts;
using LegalDocsPro.Infrastructure.Persistence.Interceptors;
using LegalDocsPro.Infrastructure.Persistence.Repositories;
using LegalDocsPro.Infrastructure.Storage;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- STARTUP CONFIGURATION VALIDATION (fail-fast) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString == "CHANGE_ME")
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured. " +
        "Set it via User Secrets (development) or environment variables (production). " +
        "See README.md for setup instructions.");
}

var jwtSecret = builder.Configuration.GetSection("JwtSettings")["Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret == "CHANGE_ME" || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is not configured or is too short (minimum 32 characters). " +
        "Set it via User Secrets (development) or environment variables (production). " +
        "See README.md for setup instructions.");
}
// --- END STARTUP VALIDATION ---

// --- INFRASTRUCTURE SERVICES ---

// TimeProvider for audit fields
builder.Services.AddSingleton(TimeProvider.System);

// Auditable Entity Interceptor
builder.Services.AddScoped<AuditableEntityInterceptor>();

// Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
    options.AddInterceptors(interceptor);
    options.UseSqlServer(
        connectionString,
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
});

// Repositories
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Domain Event Dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Authentication
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Current User Service
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// File Storage Service
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// --- APPLICATION SERVICES ---

var applicationAssembly = typeof(LegalDocsPro.Application.Features.Contracts.Commands.CreateContractCommand).Assembly;

// MediatR with Validation Behavior
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(applicationAssembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// --- JWT AUTHENTICATION ---
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

// --- API SERVICES ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LegalDocsPro API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
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

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://localhost:3000", "https://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// --- APPLY MIGRATIONS (fail-fast) ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// --- MIDDLEWARE PIPELINE ---

app.UseExceptionHandler(opt => { });

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.StartsWithSegments("/uploads"))
        {
            ctx.Context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
});

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");
app.UseAuthentication();
app.UseAuthorization();

// Redirect root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();
app.Run();