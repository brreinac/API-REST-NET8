using Microsoft.EntityFrameworkCore;
using Osigu.MedicalOrders.Api.Extensions;
using Osigu.MedicalOrders.Api.Middleware;
using Osigu.MedicalOrders.Application;
using Osigu.MedicalOrders.Infrastructure;
using Osigu.MedicalOrders.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(
    Path.Combine(Directory.GetCurrentDirectory(), "data"));

Directory.CreateDirectory(
    Path.Combine(Directory.GetCurrentDirectory(), "logs"));

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "logs",
                "application-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true);
});

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiDocumentation();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet(
    "/health",
    () => Results.Ok(new
    {
        status = "Healthy",
        service = "Osigu Medical Orders API"
    }));

app.MapControllers();

app.Run();

public partial class Program;
