# Etapa 1: compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SistemaWebPapeleria/SistemaWebPapeleria.csproj", "SistemaWebPapeleria/"]
RUN dotnet restore "SistemaWebPapeleria/SistemaWebPapeleria.csproj"

COPY SistemaWebPapeleria/ SistemaWebPapeleria/
WORKDIR /src/SistemaWebPapeleria
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: imagen final, liviana, solo con lo necesario para correr
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SistemaWebPapeleria.dll"]