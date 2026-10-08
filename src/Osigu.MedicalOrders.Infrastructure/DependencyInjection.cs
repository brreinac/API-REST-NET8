using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Infrastructure.Persistence;
using Osigu.MedicalOrders.Infrastructure.Persistence.Repositories;
using Osigu.MedicalOrders.Infrastructure.Persistence.UnitOfWork;

namespace Osigu.MedicalOrders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=data/medical-orders.db";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
