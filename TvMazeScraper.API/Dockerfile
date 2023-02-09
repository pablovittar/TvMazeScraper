#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TvMazeScraper.API/TvMazeScraper.API.csproj", "TvMazeScraper.API/"]
RUN dotnet restore "TvMazeScraper.API/TvMazeScraper.API.csproj"
COPY . .
WORKDIR "/src/TvMazeScraper.API"
RUN dotnet build "TvMazeScraper.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TvMazeScraper.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TvMazeScraper.API.dll"]