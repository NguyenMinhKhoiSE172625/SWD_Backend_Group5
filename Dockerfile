# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and restore dependencies
COPY *.sln .
COPY src/EVRentalSystem.API/EVRentalSystem.API.csproj src/EVRentalSystem.API/
COPY src/EVRentalSystem.Application/EVRentalSystem.Application.csproj src/EVRentalSystem.Application/
COPY src/EVRentalSystem.Domain/EVRentalSystem.Domain.csproj src/EVRentalSystem.Domain/
COPY src/EVRentalSystem.Infrastructure/EVRentalSystem.Infrastructure.csproj src/EVRentalSystem.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /src
RUN dotnet build src/EVRentalSystem.API/EVRentalSystem.API.csproj -c Release -o /app/build

# Publish the application
FROM build AS publish
WORKDIR /src
RUN dotnet publish src/EVRentalSystem.API/EVRentalSystem.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Expose port (Railway will set PORT environment variable dynamically)
# Railway sẽ tự động set PORT env variable khi deploy
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
# ASPNETCORE_URLS sẽ được set từ PORT env variable trong Program.cs
# Railway tự động inject PORT, app sẽ đọc và sử dụng

# Enable Swagger by default trên Railway (có thể tắt bằng env variable ENABLE_SWAGGER=false)
ENV ENABLE_SWAGGER=true

# Run the application
ENTRYPOINT ["dotnet", "EVRentalSystem.API.dll"]

