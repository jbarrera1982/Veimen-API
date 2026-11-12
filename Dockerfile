# Etapa base para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos del proyecto primero para aprovechar la caché de Docker
COPY ["Veimen API/Veimen API.csproj", "Veimen API/"]
RUN dotnet restore "Veimen API/Veimen API.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/Veimen API"
RUN dotnet build "Veimen API.csproj" -c Release -o /app/build --no-restore

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "Veimen API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# curl para el healthcheck del contenedor
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# ASP.NET escucha en HTTP 8080 dentro del contenedor.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

# Ejecutar como el usuario no-root que trae la imagen de ASP.NET.
USER $APP_UID

ENTRYPOINT ["dotnet", "Veimen API.dll"]
