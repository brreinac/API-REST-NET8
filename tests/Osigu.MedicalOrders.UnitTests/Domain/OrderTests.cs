using Osigu.MedicalOrders.Domain.Entities;
using Osigu.MedicalOrders.Domain.Enums;
using Osigu.MedicalOrders.Domain.Exceptions;

namespace Osigu.MedicalOrders.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_ShouldStartInPendingStatus()
    {
        var order = Order.Create(
            "12345",
            "Juan Perez",
            "LAB001",
            "Hemograma",
            OrderPriority.Normal);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Null(order.ProcessedAt);
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedStatusAndTimestamp()
    {
        var order = Order.Create(
            "12345",
            "Juan Perez",
            "LAB001",
            "Hemograma",
            OrderPriority.Normal);

        var processedAt = new DateTime(
            2026,
            10,
            7,
            15,
            0,
            0,
            DateTimeKind.Utc);

        order.MarkAsProcessed(processedAt);

        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.Equal(processedAt, order.ProcessedAt);
    }

    [Fact]
    public void Create_ShouldRejectMissingPatientId()
    {
        Assert.Throws<DomainException>(() =>
            Order.Create(
                "",
                "Juan Perez",
                "LAB001",
                "Hemograma",
                OrderPriority.Normal));
    }
}
