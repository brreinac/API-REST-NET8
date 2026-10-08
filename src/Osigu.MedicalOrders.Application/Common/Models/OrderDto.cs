using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Application.Common.Models;

public sealed record OrderDto(
    Guid Id,
    string PatientId,
    string PatientName,
    string ServiceCode,
    string ServiceDescription,
    string Priority,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt)
{
    public static OrderDto FromDomain(Domain.Entities.Order order) =>
        new(
            order.Id,
            order.PatientId,
            order.PatientName,
            order.ServiceCode,
            order.ServiceDescription,
            order.Priority == OrderPriority.Urgent ? "Urgente" : "Normal",
            order.Status == OrderStatus.Processed ? "Procesada" : "Pendiente",
            order.CreatedAt,
            order.ProcessedAt);
}
