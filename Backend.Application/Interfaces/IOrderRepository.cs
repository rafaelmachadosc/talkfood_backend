using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByTableAsync(int table, string? phone, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetDraftOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetNonDraftOrdersAsync(CancellationToken cancellationToken = default);
}
