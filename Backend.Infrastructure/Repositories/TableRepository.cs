using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class TableRepository : BaseRepository<Table>, ITableRepository
{
    public TableRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Table?> GetByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.QrCode == qrCode, cancellationToken);
    }

    public async Task<Table?> GetByNumberAsync(int number, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Number == number, cancellationToken);
    }
}
