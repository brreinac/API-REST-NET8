using Microsoft.EntityFrameworkCore;
using Osigu.MedicalOrders.Application.Common.Interfaces;
using Osigu.MedicalOrders.Domain.Entities;
using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAsync(
        string? patientId,
        OrderStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(patientId, status);

        return await query
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? patientId,
        OrderStatus? status,
        CancellationToken cancellationToken)
    {
        return BuildFilteredQuery(patientId, status)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPendingAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .Where(order => order.Status == OrderStatus.Pending)
            .OrderBy(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Order> BuildFilteredQuery(
        string? patientId,
        OrderStatus? status)
    {
        var query = _dbContext.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(patientId))
        {
            query = query.Where(
                order => order.PatientId == patientId.Trim());
        }

        if (status.HasValue)
        {
            query = query.Where(
                order => order.Status == status.Value);
        }

        return query;
    }
}
