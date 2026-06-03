# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["RenderDB.csproj", "./"]

# Restaurar dependencias
RUN dotnet restore "RenderDB.csproj"

# Copiar código fuente
COPY . .

# Compilar la aplicación
RUN dotnet build "RenderDB.csproj" -c Release -o /app/build

# Publicar la aplicación
RUN dotnet publish "RenderDB.csproj" -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copiar archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Exponer puerto
EXPOSE 8080

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "RenderDB.dll"]
