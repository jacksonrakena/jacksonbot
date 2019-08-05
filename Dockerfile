FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview7 AS build
ARG Configuration=Release
WORKDIR /build
COPY . ./
RUN dotnet restore
RUN dotnet build src/Abyss.Console/Abyss.Console.csproj -c $Configuration -o /app

ENTRYPOINT ["dotnet", "/app/Abyss.Console.dll", "/data"]