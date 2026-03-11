# ── Stage 1: Build ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies first (better Docker caching)
COPY src/MyApi/MyApi.csproj ./MyApi/
RUN dotnet restore ./MyApi/MyApi.csproj

# Copy all source code and publish
COPY src/MyApi/ ./MyApi/
RUN dotnet publish ./MyApi/MyApi.csproj -c Release -o /app/publish

# ── Stage 2: Runtime (much smaller image) ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApi.dll"]
