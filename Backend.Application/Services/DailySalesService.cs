using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class DailySalesService
{
    private readonly IRepository<DailySales> _dailySalesRepository;

    public DailySalesService(IRepository<DailySales> dailySalesRepository)
    {
        _dailySalesRepository = dailySalesRepository;
    }

    public async Task UpsertDailySalesAsync(DateTime date, int amount, bool isRefund = false, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        
        // Buscar registro do dia
        var allSales = await _dailySalesRepository.GetAllAsync(cancellationToken);
        var dailySales = allSales.FirstOrDefault(d => d.Date.Date == dateOnly);

        if (dailySales == null)
        {
            // Criar novo registro
            dailySales = new DailySales
            {
                Date = dateOnly,
                TotalSales = isRefund ? -amount : amount,
                TotalOrders = isRefund ? 0 : 1,
                LastUpdated = DateTime.UtcNow
            };
            await _dailySalesRepository.AddAsync(dailySales, cancellationToken);
        }
        else
        {
            // Atualizar registro existente
            if (isRefund)
            {
                dailySales.TotalSales -= amount;
            }
            else
            {
                dailySales.TotalSales += amount;
                dailySales.TotalOrders += 1;
            }
            dailySales.LastUpdated = DateTime.UtcNow;
            await _dailySalesRepository.UpdateAsync(dailySales, cancellationToken);
        }
    }

    public async Task<DailySales?> GetDailySalesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        var allSales = await _dailySalesRepository.GetAllAsync(cancellationToken);
        return allSales.FirstOrDefault(d => d.Date.Date == dateOnly);
    }

    public async Task<IEnumerable<DailySales>> GetDailySalesRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        
        var allSales = await _dailySalesRepository.GetAllAsync(cancellationToken);
        return allSales
            .Where(d => d.Date.Date >= start && d.Date.Date <= end)
            .OrderBy(d => d.Date)
            .ToList();
    }
}
