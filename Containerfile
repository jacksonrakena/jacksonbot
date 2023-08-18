FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /source

COPY . .
RUN dotnet restore src/Jacksonbot/Jacksonbot.csproj  --use-current-runtime

RUN dotnet publish -c Release -o /app --no-restore 

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build-env /app .
ENTRYPOINT ["dotnet", "Jacksonbot.dll"]