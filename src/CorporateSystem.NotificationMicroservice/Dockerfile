FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CorporateSystem.Notification.Api/CorporateSystem.Notification.Api.csproj", "CorporateSystem.Notification.Api/"]
RUN dotnet restore "CorporateSystem.Notification.Api/CorporateSystem.Notification.Api.csproj"
COPY . .
WORKDIR "/src/CorporateSystem.Notification.Api"
RUN dotnet build "CorporateSystem.Notification.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CorporateSystem.Notification.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorporateSystem.Notification.Api.dll"]
