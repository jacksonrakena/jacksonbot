FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview8 AS build
ARG Configuration=Release
WORKDIR /build
COPY . ./
RUN dotnet restore
RUN dotnet build src/Abyss.Console/Abyss.Console.csproj -c $Configuration -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview8 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Abyss.Console.dll", "/data"]
