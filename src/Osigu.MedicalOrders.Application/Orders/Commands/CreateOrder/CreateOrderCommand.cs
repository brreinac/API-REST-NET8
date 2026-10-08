using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    string PatientId,
    string PatientName,
    string ServiceCode,
    string ServiceDescription,
    OrderPriority Priority);
