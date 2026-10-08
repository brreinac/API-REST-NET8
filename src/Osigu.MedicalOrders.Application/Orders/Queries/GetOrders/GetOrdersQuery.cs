using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    string? PatientId,
    OrderStatus? Status,
    int PageNumber = 1,
    int PageSize = 20);
