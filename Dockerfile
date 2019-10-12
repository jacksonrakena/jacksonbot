FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
ARG Configuration=Release
WORKDIR /build
COPY . ./
EXPOSE 2110
RUN dotnet restore src/Abyss.Host/Abyss.Host.csproj
RUN dotnet publish src/Abyss.Host/Abyss.Host.csproj -c $Configuration -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Abyss.Host.dll", "/data"]
