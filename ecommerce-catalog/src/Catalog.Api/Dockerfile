# --- Derleme aşaması ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Catalog.Domain/Catalog.Domain.csproj", "src/Catalog.Domain/"]
COPY ["src/Catalog.Application/Catalog.Application.csproj", "src/Catalog.Application/"]
COPY ["src/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Catalog.Infrastructure/"]
COPY ["src/Catalog.Api/Catalog.Api.csproj", "src/Catalog.Api/"]
RUN dotnet restore "src/Catalog.Api/Catalog.Api.csproj"

COPY . .
RUN dotnet publish "src/Catalog.Api/Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Çalışma aşaması ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
