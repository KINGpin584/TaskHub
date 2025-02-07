# taskmangement api

## start sql server
'''

'''
# Project Overview

This project is built using .NET 9.0 and utilizes several NuGet packages to work with ASP.NET Core SignalR, Entity Framework Core with MySQL support, and more. It also references an external project (TaskManagementSystem.Core).

## Requirements

- **.NET SDK**  
  • .NET 9.0 (ensure you have this installed to build and run the app)

- **NuGet Packages**  
  • Microsoft.AspNetCore.SignalR (v1.2.0)  
  • Microsoft.EntityFrameworkCore (v8.0.2)  
  • Pomelo.EntityFrameworkCore.MySql (v8.0.2)  
  • Microsoft.EntityFrameworkCore.Design (v8.0.2)  
  • Microsoft.EntityFrameworkCore.Tools (v8.0.2)

  -**Make Migrations and Database update to Mysql_server**
  dotnet ef migrations add <MigrationName> --project <YourDataProject> --startup-project <YourStartupProject>
  dotnet ef database update --project <YourDataProject> --startup-project <YourStartupProject>
  dotnet build
  dotnet run -{to run the server on local host}

   

- **Project Dependency**  
  • TaskManagementSystem.Core 

For a quick reference of these dependencies, consult the `req.txt` file provided with the project.

## Installation

1. **Restore NuGet Packages**  
   Open a terminal in the project directory and run:


2. **Installing Requirements from req.txt**  
The `req.txt` file lists all external system and package requirements. Use it as a checklist to ensure you have the following installed:
- .NET 9.0 SDK
- The NuGet package versions specified above  

(Note: The NuGet packages are restored via `dotnet restore`; use `req.txt` to verify you have the appropriate system dependencies.)

## Docker Setup for Backend
--first add -> dotnet ef migrations add InitialCreate
Then wecan go ahead:
To compile and run the backend Docker container using Docker Compose, follow these steps:

1. Bring down any running services: docker-compose down

2. Build the updated images: docker-compose build

3. Start the containers: docker-compose up


## Docker Setup for Frontend

To run the frontend Docker container, execute the following command in your terminal:

docker rm -f task-frontend ; docker build -t task-frontend . && docker run -d -p 80:80 --name task-frontend task-frontend


## Database Configuration

The database connection settings are located in the `appsettings.json` file. By default, the connection string uses:

- **User:** root  
- **Password:** 123

If you need to change the database user name or password, edit the connection string in `appsettings.json` accordingly.

## Summary

- Verify system requirements from `req.txt`.  
- Run `dotnet restore` to install NuGet packages.  
- Use Docker Compose (`down`, `build`, `up`) to set up backend containers.  
- Use the provided command to run the frontend container.  
- Update `appsettings.json` to change default database credentials if needed.
## Api documentation Link-
https://documenter.getpostman.com/view/32040449/2sAYX8JM6m
