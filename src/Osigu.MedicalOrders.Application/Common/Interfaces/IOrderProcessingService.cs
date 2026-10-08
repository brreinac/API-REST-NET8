namespace Osigu.MedicalOrders.Application.Common.Interfaces;

public interface IOrderProcessingService
{
    Task<int> ProcessPendingOrdersAsync(CancellationToken cancellationToken);
}
