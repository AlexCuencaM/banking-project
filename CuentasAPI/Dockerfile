FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CuentasAPI/CuentasAPI.csproj", "CuentasAPI/"]
RUN dotnet restore "CuentasAPI/CuentasAPI.csproj"

COPY . .
WORKDIR /src/CuentasAPI
RUN dotnet publish "CuentasAPI.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CuentasAPI.dll"]