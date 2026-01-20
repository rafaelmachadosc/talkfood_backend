using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByTableAsync(int table, string? phone, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.Table == table);

        if (!string.IsNullOrEmpty(phone))
        {
            query = query.Where(o => o.Phone == phone);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetDraftOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.Draft == true)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
