FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CorporateSystem.ApiGateway.Api/CorporateSystem.ApiGateway.Api.csproj", "CorporateSystem.ApiGateway.Api/"]
RUN dotnet restore "CorporateSystem.ApiGateway.Api/CorporateSystem.ApiGateway.Api.csproj"
COPY . .
WORKDIR "/src/CorporateSystem.ApiGateway.Api"
RUN dotnet build "CorporateSystem.ApiGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CorporateSystem.ApiGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorporateSystem.ApiGateway.Api.dll"]
