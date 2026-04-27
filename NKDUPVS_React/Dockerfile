FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# -----------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js v20 from official source
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && npm install -g typescript

COPY . .

# Ensure script permissions (tsc error workaround)
RUN chmod +x ./nkdupvs_react.client/node_modules/.bin/tsc || true
RUN chmod +x ./nkdupvs_react.client/node_modules/.bin/vite

RUN cd nkdupvs_react.client && npm install && npm run build

RUN mkdir -p /app/wwwroot
RUN cp -r nkdupvs_react.client/dist/* /app/wwwroot/

WORKDIR /src/NKDUPVS_React.Server
RUN dotnet publish -c Release -o /app/publish

# -----------------------------------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NKDUPVS_React.Server.dll"]
