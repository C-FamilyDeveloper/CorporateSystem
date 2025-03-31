FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8091

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["CorporateSystem.Auth.Api/CorporateSystem.Auth.Api.csproj", "CorporateSystem.Auth.Api/"]
RUN dotnet restore "CorporateSystem.Auth.Api/CorporateSystem.Auth.Api.csproj"

COPY . .
WORKDIR "/src/CorporateSystem.Auth.Api"
RUN dotnet build "CorporateSystem.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CorporateSystem.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorporateSystem.Auth.Api.dll"]