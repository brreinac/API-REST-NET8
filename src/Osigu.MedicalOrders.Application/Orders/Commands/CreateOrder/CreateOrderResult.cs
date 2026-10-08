using Osigu.MedicalOrders.Application.Common.Models;

namespace Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderResult(OrderDto Order);
