using Microsoft.EntityFrameworkCore;
using OrderAppWeb.API.Context;
using OrderAppWeb.API.Profile;
using Serilog;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region MyServices

string connectionString = builder.Configuration.GetConnectionString("MySql");

builder.Services.AddDbContext<OrderDbContext>(opt =>
{
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), null);
});

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly); //Memory Cache

//Loglama
Logger log = new LoggerConfiguration()
                .WriteTo.File("logs/log.txt")
                .WriteTo.MySQL(connectionString,"logs")
                .MinimumLevel.Information()
                .CreateLogger();

builder.Host.UseSerilog(log);

#endregion



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
