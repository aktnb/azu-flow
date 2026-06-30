# syntax=docker/dockerfile:1

FROM node:24-alpine AS frontend-build
WORKDIR /src/frontend

COPY frontend/package.json frontend/pnpm-lock.yaml ./
RUN corepack enable && corepack prepare pnpm@10.17.1 --activate
RUN pnpm install --frozen-lockfile

COPY frontend/ ./
RUN pnpm build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

COPY backend/AzuFlow.Core/AzuFlow.Core.csproj backend/AzuFlow.Core/
COPY backend/AzuFlow.Azure/AzuFlow.Azure.csproj backend/AzuFlow.Azure/
COPY backend/AzuFlow.Api/AzuFlow.Api.csproj backend/AzuFlow.Api/
RUN dotnet restore backend/AzuFlow.Api/AzuFlow.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/AzuFlow.Api/AzuFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

COPY --from=frontend-build /src/frontend/dist/ /app/publish/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    Topology__SupplementFile=/config/topology-supplement.json

EXPOSE 8080

RUN mkdir -p /config

COPY --from=backend-build /app/publish/ ./

USER $APP_UID
ENTRYPOINT ["dotnet", "AzuFlow.Api.dll"]
