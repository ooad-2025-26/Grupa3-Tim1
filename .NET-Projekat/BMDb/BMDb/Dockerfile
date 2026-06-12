# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Kopiraj .csproj i restore (iskoristi Docker cache)
COPY ["BMDb/BMDb.csproj", "BMDb/"]
RUN dotnet restore "BMDb/BMDb.csproj"

# Kopiraj sve i build
COPY . .
WORKDIR "/src/BMDb"
RUN dotnet build "BMDb.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/BMDb"
RUN dotnet publish "BMDb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BMDb.dll"]