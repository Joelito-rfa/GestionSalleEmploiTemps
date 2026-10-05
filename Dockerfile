# Build stage (Ubuntu noble = OS supporte par .NET 10, bookworm n'existe plus en 10.x)
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copie des csproj + restore (cache Docker optimal)
COPY EMIT.Domain/EMIT.Domain.csproj EMIT.Domain/
COPY EMIT.Application/EMIT.Application.csproj EMIT.Application/
COPY EMIT.Infrastructure/EMIT.Infrastructure.csproj EMIT.Infrastructure/
COPY GestionSalleEmploiTemps/GestionSalleEmploiTemps.csproj GestionSalleEmploiTemps/
RUN dotnet restore GestionSalleEmploiTemps/GestionSalleEmploiTemps.csproj

# Copie du reste + publish (sans apphost natif = moins de risque segfault)
COPY . .
WORKDIR /src/GestionSalleEmploiTemps
RUN dotnet publish GestionSalleEmploiTemps.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
WORKDIR /app
COPY --from=build /app/publish .

# Limites pour instance Render Free 512 Mo : evite OOM-kill (souvent vu comme exit 139)
ENV DOTNET_EnableDiagnostics=0 \
    DOTNET_GCHeapHardLimit=1C0000000 \
    DOTNET_GCConserveMemory=9 \
    ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

# Respecte le $PORT dynamique fourni par Render (sinon health-check KO)
ENTRYPOINT ["/bin/sh", "-c", "export ASPNETCORE_URLS=http://+:${PORT:-10000} && dotnet GestionSalleEmploiTemps.dll"]
