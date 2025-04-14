FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CorporateSystem.SharedDocs.Api/CorporateSystem.SharedDocs.Api.csproj", "CorporateSystem.SharedDocs.Api/"]
COPY ["CorporateSystem.SharedDocs.Infrastructure/CorporateSystem.SharedDocs.Infrastructure.csproj", "CorporateSystem.SharedDocs.Infrastructure/"]
COPY ["CorporateSystem.SharedDocs.Domain/CorporateSystem.SharedDocs.Domain.csproj", "CorporateSystem.SharedDocs.Domain/"]
COPY ["CorporateSystem.SharedDocs.Services/CorporateSystem.SharedDocs.Services.csproj", "CorporateSystem.SharedDocs.Services/"]
RUN dotnet restore "CorporateSystem.SharedDocs.Api/CorporateSystem.SharedDocs.Api.csproj"
COPY . .
WORKDIR "/src/CorporateSystem.SharedDocs.Api"
RUN dotnet build "CorporateSystem.SharedDocs.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CorporateSystem.SharedDocs.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorporateSystem.SharedDocs.Api.dll"]
