# Usar la imagen base de ASP.NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Usar la imagen SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar los archivos de proyecto y restaurar dependencias
COPY ["TestApp.Api/TestApp.Api.csproj", "TestApp.Api/"]
COPY ["TestApp.Core/TestApp.Core.csproj", "TestApp.Core/"]
RUN dotnet restore "TestApp.Api/TestApp.Api.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/TestApp.Api"
RUN dotnet build "TestApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar la aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TestApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear el directorio para la base de datos SQLite
RUN mkdir -p /app/data

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TestApp.Api.dll"]