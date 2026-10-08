using Osigu.MedicalOrders.Application;
using Osigu.MedicalOrders.Infrastructure;
using Osigu.MedicalOrders.Worker.Workers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Directory.CreateDirectory(
    Path.Combine(Directory.GetCurrentDirectory(), "data"));

Directory.CreateDirectory(
    Path.Combine(Directory.GetCurrentDirectory(), "logs"));

builder.Services.AddSerilog((_, loggerConfiguration) =>
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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.SectionName));

builder.Services.AddHostedService<OrderProcessingWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<
            Osigu.MedicalOrders.Infrastructure.Persistence.ApplicationDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

await host.RunAsync();
