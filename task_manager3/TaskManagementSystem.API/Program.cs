using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Services;
using TaskManagementSystem.API.Hubs;
using System.Text.Json.Serialization;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<TaskPriorityService>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<TaskManagementContext>(options => 
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)  // This is required for Pomelo
    )
);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddSignalR(); 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapHub<TaskHub>("/taskHub");
app.MapControllers();
app.Run();