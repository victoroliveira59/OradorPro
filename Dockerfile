# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Directory.Build.props", "./"]
COPY ["AgendaPalestras.sln", "./"]
COPY ["src/AgendaPalestras.Domain/AgendaPalestras.Domain.csproj", "src/AgendaPalestras.Domain/"]
COPY ["src/AgendaPalestras.Application/AgendaPalestras.Application.csproj", "src/AgendaPalestras.Application/"]
COPY ["src/AgendaPalestras.Infrastructure/AgendaPalestras.Infrastructure.csproj", "src/AgendaPalestras.Infrastructure/"]
COPY ["src/AgendaPalestras.Web/AgendaPalestras.Web.csproj", "src/AgendaPalestras.Web/"]

RUN dotnet restore "src/AgendaPalestras.Web/AgendaPalestras.Web.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/AgendaPalestras.Web"
RUN dotnet publish "AgendaPalestras.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0
EXPOSE 8080

COPY --from=build /app/publish .

# Execute with non-root user (app user in .NET 8 container images)
USER $APP_UID

ENTRYPOINT ["dotnet", "AgendaPalestras.Web.dll"]
