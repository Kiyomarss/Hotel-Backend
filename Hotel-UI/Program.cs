using Entities;
using Hotel_Infrastructure.DbContext;
using Hotel_UI;
using Hotel_UI.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
        .ReadFrom.Services(services); //read out current app's services and make them available to serilog
} );

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseSerilogRequestLogging();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

app.MapControllers();

app.UseCors("AllowSpecificOrigin");

app.Run();