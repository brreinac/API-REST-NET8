using Microsoft.Extensions.Options;
using Osigu.MedicalOrders.Application.Common.Interfaces;

namespace Osigu.MedicalOrders.Worker.Workers;

public sealed class OrderProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkerOptions _options;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<WorkerOptions> options,
        ILogger<OrderProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Order processing worker started. Interval: {IntervalSeconds}s.",
            _options.IntervalSeconds);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_options.IntervalSeconds));

        await ProcessOrdersAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessOrdersAsync(stoppingToken);
        }

        _logger.LogInformation("Order processing worker stopped.");
    }

    private async Task ProcessOrdersAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var processingService = scope.ServiceProvider
                .GetRequiredService<IOrderProcessingService>();

            var processedCount =
                await processingService.ProcessPendingOrdersAsync(
                    cancellationToken);

            if (processedCount > 0)
            {
                _logger.LogInformation(
                    "Worker processed {ProcessedCount} pending order(s).",
                    processedCount);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error while processing pending orders.");
        }
    }
}

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    public int IntervalSeconds { get; set; } = 5;
}
