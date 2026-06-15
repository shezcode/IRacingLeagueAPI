# Multi-stage build for the DWES2 API.
# Build context is the solution root (IRacingLeagueApi/) so every referenced project
# — Models, Data, Data.EF, Business, API — is available to restore/publish.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish API/IRacingLeague.API.csproj -c Release -o /app

# ASP.NET runtime image (not the plain runtime): .NET 8 Kestrel listens on 8080 here.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "IRacingLeague.API.dll"]
