# Stage 1: Build Angular frontend
FROM node:20-alpine AS ui-build
WORKDIR /app/ui
COPY ui/package*.json ./
RUN npm install --ignore-scripts
COPY ui/ ./
RUN npx ng build --configuration production

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /app
COPY RecipeVault.sln ./
COPY src/ ./src/
RUN dotnet publish src/RecipeVault.WebApi/RecipeVault.WebApi.csproj \
    -c Release -o /out

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=api-build /out ./
COPY --from=ui-build /app/ui/dist/ui/browser ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RecipeVault.WebApi.dll"]
