# Café Tracker — imagem única (backend .NET 10 que serve o dashboard SAP UI5).
# Build multi-stage: (1) builda o frontend, (2) publica o backend já com o front
# embutido, (3) imagem final só com o runtime. Traz tudo o que precisa — quem
# hospeda só precisa de Docker (sem instalar .NET nem Node).

# ===== Estágio 1: build do frontend (SAP UI5) =====
FROM node:20-bookworm AS frontend
WORKDIR /front
COPY frontend/ ./
# Baixa o @ui5/cli e constrói o app self-contained (inclui o SAPUI5) em /front/dist.
RUN npm install && npm run build

# ===== Estágio 2: build/publish do backend (.NET 10) =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY backend/ ./
# Coloca o frontend já buildado no wwwroot da API (a API serve o dashboard).
COPY --from=frontend /front/dist ./src/CafeTracker.Api/wwwroot
# Publica só o backend (o front já está pronto; -p:SkipFrontend=true pula o npm).
RUN dotnet publish src/CafeTracker.Api/CafeTracker.Api.csproj \
    -c Release -o /app -p:SkipFrontend=true

# ===== Estágio 3: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend /app ./
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CafeTracker.Api.dll"]
