# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Force the application to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# === CHANGED: Install curl right here in the base image ===
RUN apt-get update && apt-get install -y curl
# ==========================================================

# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the csproj first to restore dependencies
COPY ["PHP.QARAdjustmentTool.API.csproj", "./"]
RUN dotnet restore "./PHP.QARAdjustmentTool.API.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet build "PHP.QARAdjustmentTool.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PHP.QARAdjustmentTool.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Simple Docker-level healthcheck for container runtime
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
	CMD curl -f http://localhost:8080/health || exit 1

# ENTRYPOINT must be the last command
ENTRYPOINT ["dotnet", "PHP.QARAdjustmentTool.API.dll"]