FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "src/Shared.Contracts/"]
COPY ["src/Ordering.Domain/Ordering.Domain.csproj", "src/Ordering.Domain/"]
COPY ["src/Ordering.Application/Ordering.Application.csproj", "src/Ordering.Application/"]
COPY ["src/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "src/Ordering.Infrastructure/"]
COPY ["src/Ordering.Api/Ordering.Api.csproj", "src/Ordering.Api/"]
RUN dotnet restore "src/Ordering.Api/Ordering.Api.csproj"
COPY . .
RUN dotnet publish "src/Ordering.Api/Ordering.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
