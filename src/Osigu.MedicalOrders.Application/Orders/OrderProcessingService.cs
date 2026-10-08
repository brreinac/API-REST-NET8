using Microsoft.Extensions.Logging;
using Osigu.MedicalOrders.Application.Common.Interfaces;

namespace Osigu.MedicalOrders.Application.Orders;

public sealed class OrderProcessingService : IOrderProcessingService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderProcessingService> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> ProcessPendingOrdersAsync(
        CancellationToken cancellationToken)
    {
        var pendingOrders = await _orderRepository.GetPendingAsync(
            cancellationToken);

        if (pendingOrders.Count == 0)
        {
            return 0;
        }

        var processedCount = 0;

        foreach (var order in pendingOrders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Processing order {OrderId} with priority {Priority}.",
                order.Id,
                order.Priority);

            await Task.Delay(
                TimeSpan.FromMilliseconds(250),
                cancellationToken);

            order.MarkAsProcessed();

            processedCount++;

            _logger.LogInformation(
                "Order {OrderId} changed to {Status}.",
                order.Id,
                order.Status);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return processedCount;
    }
}
