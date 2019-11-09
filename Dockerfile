FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /build
COPY . ./
RUN dotnet publish src/Abyss.Hosts.Default/Abyss.Hosts.Default.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Abyss.Hosts.Default.dll", "/data"]
