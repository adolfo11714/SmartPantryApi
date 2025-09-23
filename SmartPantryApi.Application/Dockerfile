FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SmartPantryApi.Application/SmartPantryApi.Application.csproj", "SmartPantryApi.Application/"]
RUN dotnet restore "SmartPantryApi.Application/SmartPantryApi.Application.csproj"
COPY . .
WORKDIR "/src/SmartPantryApi.Application"
RUN dotnet build "./SmartPantryApi.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SmartPantryApi.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartPantryApi.Application.dll"]
