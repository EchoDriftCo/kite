# Stage 1: Build Angular frontend
FROM node:20-alpine AS ui-build
WORKDIR /app/ui
COPY ui/package*.json ./
RUN npm install --ignore-scripts
COPY ui/ ./
RUN npx ng build --configuration production

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
# Accept VERSION so the assembly is stamped correctly (MinVer can't run; .git is excluded)
ARG VERSION=1.0.0-local
WORKDIR /app
COPY RecipeVault.sln ./
COPY src/ ./src/
RUN dotnet publish src/RecipeVault.WebApi/RecipeVault.WebApi.csproj \
    -c Release -o /out \
    -p:MinVerSkip=true \
    -p:Version=${VERSION} \
    -p:AssemblyInformationalVersion=${VERSION}

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=api-build /out ./
COPY --from=ui-build /app/ui/dist/ui/browser ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Build metadata - injected at build time via --build-arg
# ASP.NET Core maps double-underscore env vars to config keys (BUILD__VERSION → Build:Version)
ARG VERSION=1.0.0-local
ARG GIT_SHA=local
ARG BUILD_TIMESTAMP=1970-01-01T00:00:00Z
ENV BUILD__VERSION=${VERSION}
ENV BUILD__TAG=${GIT_SHA}
ENV BUILD__TIMESTAMP=${BUILD_TIMESTAMP}

ENTRYPOINT ["dotnet", "RecipeVault.WebApi.dll"]
