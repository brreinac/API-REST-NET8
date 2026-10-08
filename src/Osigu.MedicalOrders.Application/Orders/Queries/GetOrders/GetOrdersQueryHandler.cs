using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Application.Common.Models;

namespace Osigu.MedicalOrders.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> HandleAsync(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var orders = await _orderRepository.GetAsync(
            query.PatientId,
            query.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var totalCount = await _orderRepository.CountAsync(
            query.PatientId,
            query.Status,
            cancellationToken);

        var items = orders
            .Select(OrderDto.FromDomain)
            .ToList();

        return new PagedResult<OrderDto>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }
}
