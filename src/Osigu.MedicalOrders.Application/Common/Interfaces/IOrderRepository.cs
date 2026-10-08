using Osigu.MedicalOrders.Domain.Entities;
using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetAsync(
        string? patientId,
        OrderStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? patientId,
        OrderStatus? status,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetPendingAsync(
        CancellationToken cancellationToken);
}
