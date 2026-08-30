# --- ETAPA 1: BUILD ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar los archivos de los proyectos existentes
COPY ["LegalDocsPro.Api/LegalDocsPro.Api.csproj", "LegalDocsPro.Api/"]
COPY ["LegalDocsPro.Application/LegalDocsPro.Application.csproj", "LegalDocsPro.Application/"]
COPY ["LegalDocsPro.Domain/LegalDocsPro.Domain.csproj", "LegalDocsPro.Domain/"]
COPY ["LegalDocsPro.Infrastructure/LegalDocsPro.Infrastructure.csproj", "LegalDocsPro.Infrastructure/"]
COPY ["tests/LegalDocsPro.Domain.Tests/LegalDocsPro.Domain.Tests.csproj", "tests/LegalDocsPro.Domain.Tests/"]

# Restaurar dependencias directamente desde el proyecto de la API
RUN dotnet restore "LegalDocsPro.Api/LegalDocsPro.Api.csproj"

# Copiar el resto del código fuente
COPY . .

# Compilar y publicar
WORKDIR "/src/LegalDocsPro.Api"
RUN dotnet publish "LegalDocsPro.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- ETAPA 2: RUNTIME ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LegalDocsPro.Api.dll"]
