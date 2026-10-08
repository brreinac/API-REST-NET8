using Microsoft.Extensions.DependencyInjection;
using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Application.Orders;
using Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;
using Osigu.MedicalOrders.Application.Orders.Queries.GetOrder;
using Osigu.MedicalOrders.Application.Orders.Queries.GetOrders;

namespace Osigu.MedicalOrders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateOrderCommandHandler>();
        services.AddScoped<GetOrderQueryHandler>();
        services.AddScoped<GetOrdersQueryHandler>();
        services.AddScoped<IOrderProcessingService, OrderProcessingService>();

        return services;
    }
}
