# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie des csproj + restore (cache Docker optimal)
COPY EMIT.Domain/EMIT.Domain.csproj EMIT.Domain/
COPY EMIT.Application/EMIT.Application.csproj EMIT.Application/
COPY EMIT.Infrastructure/EMIT.Infrastructure.csproj EMIT.Infrastructure/
COPY GestionSalleEmploiTemps/GestionSalleEmploiTemps.csproj GestionSalleEmploiTemps/
RUN dotnet restore GestionSalleEmploiTemps/GestionSalleEmploiTemps.csproj

# Copie du reste + publish
COPY . .
WORKDIR /src/GestionSalleEmploiTemps
RUN dotnet publish GestionSalleEmploiTemps.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render fournit $PORT (10000 par defaut). ASPNETCORE_URLS est surcharge via env Render.
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "GestionSalleEmploiTemps.dll"]
