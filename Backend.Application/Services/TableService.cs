using Backend.Application.DTOs.Table;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class TableService
{
    private readonly ITableRepository _tableRepository;

    public TableService(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<TableDto> CreateTableAsync(int number, CancellationToken cancellationToken = default)
    {
        var existingTable = await _tableRepository.GetByNumberAsync(number, cancellationToken);
        if (existingTable != null)
        {
            throw new InvalidOperationException("Mesa com este número já existe");
        }

        var qrCode = Guid.NewGuid().ToString("N")[..16].ToUpper();

        var table = new Table
        {
            Number = number,
            QrCode = qrCode,
            IsActive = true
        };

        var createdTable = await _tableRepository.AddAsync(table, cancellationToken);

        return new TableDto
        {
            Id = createdTable.Id,
            Number = createdTable.Number,
            QrCode = createdTable.QrCode,
            IsActive = createdTable.IsActive,
            CreatedAt = createdTable.CreatedAt
        };
    }

    public async Task<IEnumerable<TableDto>> GetAllTablesAsync(CancellationToken cancellationToken = default)
    {
        var tables = await _tableRepository.GetAllAsync(cancellationToken);
        return tables.Select(t => new TableDto
        {
            Id = t.Id,
            Number = t.Number,
            QrCode = t.QrCode,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        });
    }

    public async Task<TableDto> GetTableByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByQrCodeAsync(qrCode, cancellationToken);
        if (table == null)
        {
            throw new KeyNotFoundException("Mesa não encontrada");
        }

        return new TableDto
        {
            Id = table.Id,
            Number = table.Number,
            QrCode = table.QrCode,
            IsActive = table.IsActive,
            CreatedAt = table.CreatedAt
        };
    }

    public async Task<TableDto> UpdateTableAsync(Guid id, int? number, bool? isActive, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken);
        if (table == null)
        {
            throw new KeyNotFoundException("Mesa não encontrada");
        }

        if (number.HasValue && number.Value != table.Number)
        {
            var existingTable = await _tableRepository.GetByNumberAsync(number.Value, cancellationToken);
            if (existingTable != null && existingTable.Id != id)
            {
                throw new InvalidOperationException("Mesa com este número já existe");
            }
            table.Number = number.Value;
        }

        if (isActive.HasValue)
        {
            table.IsActive = isActive.Value;
        }

        await _tableRepository.UpdateAsync(table, cancellationToken);

        return new TableDto
        {
            Id = table.Id,
            Number = table.Number,
            QrCode = table.QrCode,
            IsActive = table.IsActive,
            CreatedAt = table.CreatedAt
        };
    }

    public async Task DeleteTableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken);
        if (table == null)
        {
            throw new KeyNotFoundException("Mesa não encontrada");
        }

        await _tableRepository.DeleteAsync(table, cancellationToken);
    }
}
