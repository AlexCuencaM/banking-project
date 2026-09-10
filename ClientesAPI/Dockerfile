FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ClientesAPI/ClientesAPI.csproj", "ClientesAPI/"]
RUN dotnet restore "ClientesAPI/ClientesAPI.csproj"

COPY . .
WORKDIR /src/ClientesAPI
RUN dotnet publish "ClientesAPI.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "ClientesAPI.dll"]