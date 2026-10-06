FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Atelier-backend.csproj ./
RUN dotnet restore Atelier-backend.csproj

COPY . ./
RUN dotnet publish Atelier-backend.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

COPY --from=build /app/publish ./

ENTRYPOINT ["sh", "-c", "exec dotnet Atelier-backend.dll --urls http://0.0.0.0:${PORT:-10000}"]
