# ===== ETAPA 1: compilar y publicar el proyecto =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo el .csproj: asi Docker guarda en cache la restauracion de paquetes
COPY SistemaWebPapeleria.csproj ./
RUN dotnet restore

# Despues el resto del codigo, y se publica en modo Release
COPY . .
RUN dotnet publish SistemaWebPapeleria.csproj -c Release -o /app/publish --no-restore

# ===== ETAPA 2: imagen final (solo lo necesario para ejecutar) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Libreria de Linux para generar PDF (wkhtmltopdf), fuentes y zona horaria
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates fontconfig fonts-liberation tzdata \
    && curl -fsSL -o /tmp/wkhtmltox.deb https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6.1-3/wkhtmltox_0.12.6.1-3.jammy_amd64.deb \
    && apt-get install -y --no-install-recommends /tmp/wkhtmltox.deb \
    && ldconfig \
    && rm -rf /tmp/wkhtmltox.deb /var/lib/apt/lists/*

ENV LD_LIBRARY_PATH=/usr/local/lib
# El appsettings.json fija https://localhost:7054; esta variable lo reemplaza dentro del contenedor
ENV Urls=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SistemaWebPapeleria.dll"]