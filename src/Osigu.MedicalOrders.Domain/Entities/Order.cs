using Osigu.MedicalOrders.Domain.Enums;
using Osigu.MedicalOrders.Domain.Exceptions;

namespace Osigu.MedicalOrders.Domain.Entities;

public sealed class Order
{
    private Order()
    {
    }

    public Guid Id { get; private set; }
    public string PatientId { get; private set; } = string.Empty;
    public string PatientName { get; private set; } = string.Empty;
    public string ServiceCode { get; private set; } = string.Empty;
    public string ServiceDescription { get; private set; } = string.Empty;
    public OrderPriority Priority { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public static Order Create(
        string patientId,
        string patientName,
        string serviceCode,
        string serviceDescription,
        OrderPriority priority,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(patientId))
        {
            throw new DomainException("PatientId is required.");
        }

        if (string.IsNullOrWhiteSpace(serviceCode))
        {
            throw new DomainException("ServiceCode is required.");
        }

        if (!Enum.IsDefined(priority))
        {
            throw new DomainException("Priority must be Normal or Urgent.");
        }

        return new Order
        {
            Id = Guid.NewGuid(),
            PatientId = patientId.Trim(),
            PatientName = patientName?.Trim() ?? string.Empty,
            ServiceCode = serviceCode.Trim(),
            ServiceDescription = serviceDescription?.Trim() ?? string.Empty,
            Priority = priority,
            Status = OrderStatus.Pending,
            CreatedAt = createdAtUtc ?? DateTime.UtcNow
        };
    }

    public void MarkAsProcessed(DateTime? processedAtUtc = null)
    {
        if (Status == OrderStatus.Processed)
        {
            return;
        }

        Status = OrderStatus.Processed;
        ProcessedAt = processedAtUtc ?? DateTime.UtcNow;
    }
}
