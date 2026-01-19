FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY VirtualPM/VirtualPM.csproj VirtualPM/
RUN dotnet restore VirtualPM/VirtualPM.csproj

# Copy source code and build
COPY VirtualPM/ VirtualPM/
WORKDIR /src/VirtualPM
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create directory for external config files
RUN mkdir -p /app/config

COPY --from=build /app/publish .

# Environment variables for configuration (override via docker run -e)
ENV ASPNETCORE_URLS=http://+:8080
ENV VirtualPM__CronSchedule="0 13 * * 0-4"
ENV TZ=UTC

EXPOSE 8080

ENTRYPOINT ["dotnet", "VirtualPM.dll"]
