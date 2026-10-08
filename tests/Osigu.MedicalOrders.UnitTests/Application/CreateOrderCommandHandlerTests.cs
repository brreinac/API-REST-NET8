using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;
using Osigu.MedicalOrders.Domain.Entities;
using Osigu.MedicalOrders.Domain.Enums;
using Osigu.MedicalOrders.Domain.Exceptions;

namespace Osigu.MedicalOrders.UnitTests.Application;

public sealed class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateValidOrderAsPending()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrderCommandHandler(
            repository,
            unitOfWork);

        var command = new CreateOrderCommand(
            "12345",
            "Juan Perez",
            "LAB001",
            "Hemograma",
            OrderPriority.Normal);

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Order.Id);
        Assert.Equal("Pendiente", result.Order.Status);
        Assert.Equal(1, repository.AddedOrders.Count);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectInvalidOrder()
    {
        var handler = new CreateOrderCommandHandler(
            new FakeOrderRepository(),
            new FakeUnitOfWork());

        var command = new CreateOrderCommand(
            "",
            "Juan Perez",
            "",
            "Hemograma",
            OrderPriority.Normal);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(
                command,
                CancellationToken.None));

        Assert.Contains(
            "PatientId is required.",
            exception.Errors);

        Assert.Contains(
            "ServiceCode is required.",
            exception.Errors);
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public List<Order> AddedOrders { get; } = [];

        public Task AddAsync(
            Order order,
            CancellationToken cancellationToken)
        {
            AddedOrders.Add(order);
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                AddedOrders.SingleOrDefault(order => order.Id == id));

        public Task<IReadOnlyList<Order>> GetAsync(
            string? patientId,
            OrderStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Order>>(
                AddedOrders);

        public Task<int> CountAsync(
            string? patientId,
            OrderStatus? status,
            CancellationToken cancellationToken) =>
            Task.FromResult(AddedOrders.Count);

        public Task<IReadOnlyList<Order>> GetPendingAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Order>>(
                AddedOrders
                    .Where(order => order.Status == OrderStatus.Pending)
                    .ToList());
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }
}
