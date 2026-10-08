using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Application.Common.Models;
using Osigu.MedicalOrders.Domain.Entities;
using Osigu.MedicalOrders.Domain.Exceptions;

namespace Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateOrderResult> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var validationErrors = CreateOrderValidator.Validate(command);

        if (validationErrors.Count > 0)
        {
            throw new ValidationException(validationErrors);
        }

        var order = Order.Create(
            command.PatientId,
            command.PatientName,
            command.ServiceCode,
            command.ServiceDescription,
            command.Priority);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(OrderDto.FromDomain(order));
    }
}
