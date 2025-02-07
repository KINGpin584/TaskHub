# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file if you have one
COPY *.sln .

# Copy all project files first for better layer caching
COPY ["TaskManagementSystem.API/TaskManagementSystem.API.csproj", "TaskManagementSystem.API/"]
COPY ["TaskManagementSystem.Core/TaskManagementSystem.Core.csproj", "TaskManagementSystem.Core/"]

# Restore NuGet packages
RUN dotnet restore "TaskManagementSystem.API/TaskManagementSystem.API.csproj"

# Add MySQL provider
RUN dotnet add "TaskManagementSystem.API/TaskManagementSystem.API.csproj" package Pomelo.EntityFrameworkCore.MySql

# Copy the rest of the source code
COPY . .

# Set working directory to API project
WORKDIR "/src/TaskManagementSystem.API"

# Build the application
RUN dotnet build "TaskManagementSystem.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TaskManagementSystem.API.csproj" -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "TaskManagementSystem.API.dll"]