# --- Derleme aşaması ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce csproj'ları kopyalayıp restore etmek katman önbelleğini iyi kullanır.
COPY ["src/Identity.Domain/Identity.Domain.csproj", "src/Identity.Domain/"]
COPY ["src/Identity.Application/Identity.Application.csproj", "src/Identity.Application/"]
COPY ["src/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Identity.Infrastructure/"]
COPY ["src/Identity.Api/Identity.Api.csproj", "src/Identity.Api/"]
RUN dotnet restore "src/Identity.Api/Identity.Api.csproj"

COPY . .
RUN dotnet publish "src/Identity.Api/Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Çalışma aşaması ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
