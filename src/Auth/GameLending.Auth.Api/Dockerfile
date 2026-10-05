FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/Auth/GameLending.Auth.Api/GameLending.Auth.Api.csproj -c Release -o /app --no-self-contained
FROM mcr.microsoft.com/dotnet/aspnet:10.0.12-noble
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "GameLending.Auth.Api.dll"]
