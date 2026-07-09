FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Gateway.Api/Gateway.Api.csproj", "src/Gateway.Api/"]
RUN dotnet restore "src/Gateway.Api/Gateway.Api.csproj"
COPY . .
RUN dotnet publish "src/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
