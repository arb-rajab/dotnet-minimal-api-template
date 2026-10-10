# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ProjectName.Api/ProjectName.Api.csproj src/ProjectName.Api/
RUN dotnet restore src/ProjectName.Api/ProjectName.Api.csproj

COPY src/ProjectName.Api/ src/ProjectName.Api/
RUN dotnet publish src/ProjectName.Api/ProjectName.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --gid 10001 projectname \
    && useradd --uid 10001 --gid projectname --shell /usr/sbin/nologin --no-create-home projectname

COPY --from=build /app/publish .

USER projectname

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectName.Api.dll"]
