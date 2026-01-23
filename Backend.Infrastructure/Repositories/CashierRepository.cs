using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class CashierRepository : BaseRepository<Cashier>, IRepository<Cashier>
{
    public CashierRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Cashier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Movements)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Cashier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Movements)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
