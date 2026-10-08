using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Application.Common.Models;
using Osigu.MedicalOrders.Domain.Exceptions;

namespace Osigu.MedicalOrders.Application.Orders.Queries.GetOrder;

public sealed class GetOrderQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> HandleAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            query.Id,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                $"Order '{query.Id}' was not found.");
        }

        return OrderDto.FromDomain(order);
    }
}
