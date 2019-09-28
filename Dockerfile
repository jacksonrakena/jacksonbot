FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
ARG Configuration=Release
WORKDIR /build
COPY . ./
RUN dotnet restore src/Abyss.Console/Abyss.Console.csproj
RUN dotnet build src/Abyss.Console/Abyss.Console.csproj -c $Configuration -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Abyss.Console.dll", "/data"]
