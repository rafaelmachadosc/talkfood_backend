using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface ITableRepository : IRepository<Table>
{
    Task<Table?> GetByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default);
    Task<Table?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);
}
