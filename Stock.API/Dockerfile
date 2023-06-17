#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Stock.API/Stockk.API.csproj", "Stock.API/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Stock.API/Stockk.API.csproj"
COPY . .
WORKDIR "/src/Stock.API"
RUN dotnet build "Stockk.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stockk.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stockk.API.dll"]