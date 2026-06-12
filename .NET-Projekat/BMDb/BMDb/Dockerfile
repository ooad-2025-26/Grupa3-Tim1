FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Kopiramo SVE fajlove s repozitorija direktno u Docker
COPY . .

# 2. Pokrećemo restore, build i publish tako što damo DIREKTNU i tačnu putanju do fajla
#    (izbjegavamo točku na početku foldera tako što ga gađamo direktno od /src)
RUN dotnet restore "/src/.NET-Projekat/BMDb/BMDb/BMDb.csproj"
RUN dotnet build "/src/.NET-Projekat/BMDb/BMDb/BMDb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "/src/.NET-Projekat/BMDb/BMDb/BMDb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BMDb.dll"]