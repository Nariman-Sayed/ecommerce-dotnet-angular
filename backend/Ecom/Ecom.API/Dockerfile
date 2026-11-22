# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0.428 AS build
WORKDIR /src

# Restore against the project files only so source edits do not invalidate the
# NuGet cache layer. Ecom.slnx is not readable by the .NET 8 SDK, so the API
# project is built directly.
COPY Directory.Build.props Directory.Packages.props ./
COPY Ecom.Core/Ecom.Core.csproj Ecom.Core/
COPY Ecom.Application/Ecom.Application.csproj Ecom.Application/
COPY Ecom.infrastructure/Ecom.infrastructure.csproj Ecom.infrastructure/
COPY Ecom.API/Ecom.API.csproj Ecom.API/
RUN dotnet restore Ecom.API/Ecom.API.csproj

COPY . .
RUN dotnet publish Ecom.API/Ecom.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0.34 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Program.cs loads Ecom.API.xml at startup, so fail the build if publish
# dropped it. The upload folder must exist and belong to the non-root user.
RUN test -f Ecom.API.xml \
    && mkdir -p /app/wwwroot \
    && chown -R app:app /app

VOLUME ["/app/wwwroot"]
USER app
ENTRYPOINT ["dotnet", "Ecom.API.dll"]
