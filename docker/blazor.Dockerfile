# ── Stage 1: Build ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/MyBlazor/MyBlazor.csproj ./MyBlazor/
RUN dotnet restore ./MyBlazor/MyBlazor.csproj

COPY src/MyBlazor/ ./MyBlazor/
RUN dotnet publish ./MyBlazor/MyBlazor.csproj -c Release -o /app/publish

# ── Stage 2: Serve with Nginx (for Blazor WASM) ─────────────────────────
FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80

# NOTE: For Blazor Server, replace Stage 2 with:
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/publish .
# EXPOSE 8080
# ENTRYPOINT ["dotnet", "MyBlazor.dll"]
