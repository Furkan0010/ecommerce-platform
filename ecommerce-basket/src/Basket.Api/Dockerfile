FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Basket.Domain/Basket.Domain.csproj", "src/Basket.Domain/"]
COPY ["src/Basket.Application/Basket.Application.csproj", "src/Basket.Application/"]
COPY ["src/Basket.Infrastructure/Basket.Infrastructure.csproj", "src/Basket.Infrastructure/"]
COPY ["src/Basket.Api/Basket.Api.csproj", "src/Basket.Api/"]
RUN dotnet restore "src/Basket.Api/Basket.Api.csproj"
COPY . .
RUN dotnet publish "src/Basket.Api/Basket.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Basket.Api.dll"]
