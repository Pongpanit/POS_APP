# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy .csproj file first
COPY POS_APP/POS_APP.csproj POS_APP/
RUN dotnet restore POS_APP/POS_APP.csproj

# Copy the rest of the source code
COPY . .

# Publish the app
WORKDIR /src/POS_APP
RUN dotnet publish POS_APP.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "POS_APP.dll"]
