using Microsoft.AspNetCore.Mvc;
using Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;
using Osigu.MedicalOrders.Application.Orders.Queries.GetOrder;
using Osigu.MedicalOrders.Application.Orders.Queries.GetOrders;
using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly CreateOrderCommandHandler _createOrderHandler;
    private readonly GetOrderQueryHandler _getOrderHandler;
    private readonly GetOrdersQueryHandler _getOrdersHandler;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        CreateOrderCommandHandler createOrderHandler,
        GetOrderQueryHandler getOrderHandler,
        GetOrdersQueryHandler getOrdersHandler,
        ILogger<OrdersController> logger)
    {
        _createOrderHandler = createOrderHandler;
        _getOrderHandler = getOrderHandler;
        _getOrdersHandler = getOrdersHandler;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new medical order in Pending status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var priority = ParsePriority(request.Priority);

        var command = new CreateOrderCommand(
            request.PatientId,
            request.PatientName ?? string.Empty,
            request.ServiceCode,
            request.ServiceDescription ?? string.Empty,
            priority);

        var result = await _createOrderHandler.HandleAsync(
            command,
            cancellationToken);

        _logger.LogInformation(
            "Medical order {OrderId} created for patient {PatientId}.",
            result.Order.Id,
            result.Order.PatientId);

        var response = CreateOrderResponse.From(result.Order);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    /// <summary>
    /// Gets medical orders using optional patient and status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? patientId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var parsedStatus = ParseStatus(status);

        var result = await _getOrdersHandler.HandleAsync(
            new GetOrdersQuery(
                patientId,
                parsedStatus,
                pageNumber,
                pageSize),
            cancellationToken);

        var response = new PagedOrderResponse(
            result.Items.Select(OrderResponse.From).ToList(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);

        return Ok(response);
    }

    /// <summary>
    /// Gets a medical order by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _getOrderHandler.HandleAsync(
            new GetOrderQuery(id),
            cancellationToken);

        return Ok(OrderResponse.From(order));
    }

    private static OrderPriority ParsePriority(string? value)
    {
        if (string.Equals(value, "Normal", StringComparison.OrdinalIgnoreCase))
        {
            return OrderPriority.Normal;
        }

        if (string.Equals(value, "Urgente", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Urgent", StringComparison.OrdinalIgnoreCase))
        {
            return OrderPriority.Urgent;
        }

        throw new ArgumentException(
            "Priority must be 'Normal' or 'Urgente'.",
            nameof(value));
    }

    private static OrderStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (string.Equals(value, "Pendiente", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return OrderStatus.Pending;
        }

        if (string.Equals(value, "Procesada", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Processed", StringComparison.OrdinalIgnoreCase))
        {
            return OrderStatus.Processed;
        }

        throw new ArgumentException(
            "Status must be 'Pendiente' or 'Procesada'.",
            nameof(value));
    }
}

public sealed record CreateOrderRequest(
    string? PatientId,
    string? PatientName,
    string? ServiceCode,
    string? ServiceDescription,
    string? Priority);

public sealed record CreateOrderResponse(
    Guid Id,
    string PatientId,
    string PatientName,
    string ServiceCode,
    string ServiceDescription,
    string Priority,
    string Status,
    DateTime CreatedAt)
{
    public static CreateOrderResponse From(
        Application.Common.Models.OrderDto order) =>
        new(
            order.Id,
            order.PatientId,
            order.PatientName,
            order.ServiceCode,
            order.ServiceDescription,
            order.Priority,
            order.Status,
            order.CreatedAt);
}

public sealed record OrderResponse(
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
    public static OrderResponse From(
        Application.Common.Models.OrderDto order) =>
        new(
            order.Id,
            order.PatientId,
            order.PatientName,
            order.ServiceCode,
            order.ServiceDescription,
            order.Priority,
            order.Status,
            order.CreatedAt,
            order.ProcessedAt);
}

public sealed record PagedOrderResponse(
    IReadOnlyList<OrderResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
