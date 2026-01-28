using Backend.Application.DTOs.Cashier;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class CashierService
{
    private readonly IRepository<Cashier> _cashierRepository;
    private readonly DailySalesService _dailySalesService;

    public CashierService(IRepository<Cashier> cashierRepository, DailySalesService dailySalesService)
    {
        _cashierRepository = cashierRepository;
        _dailySalesService = dailySalesService;
    }

    public async Task<CashierDto> GetCashierStatusAsync(CancellationToken cancellationToken = default)
    {
        var activeCashier = await _cashierRepository.FirstOrDefaultAsync(
            c => c.ClosedAt == null,
            cancellationToken
        );

        if (activeCashier == null)
        {
            return new CashierDto
            {
                IsOpen = false
            };
        }

        // Garantir que IsOpen está sincronizado com ClosedAt
        var isOpen = activeCashier.ClosedAt == null;

        return new CashierDto
        {
            Id = activeCashier.Id,
            IsOpen = isOpen,
            OpenedAt = activeCashier.OpenedAt,
            ClosedAt = activeCashier.ClosedAt,
            OpenedBy = activeCashier.OpenedBy,
            InitialAmount = activeCashier.InitialAmount,
            CurrentAmount = activeCashier.CurrentAmount,
            TotalSales = activeCashier.TotalSales,
            TotalOrders = activeCashier.TotalOrders
        };
    }

    public async Task<CashierDto> OpenCashierAsync(int initialAmount, Guid userId, CancellationToken cancellationToken = default)
    {
        var activeCashier = await _cashierRepository.FirstOrDefaultAsync(
            c => c.ClosedAt == null,
            cancellationToken
        );

        if (activeCashier != null)
        {
            throw new InvalidOperationException("Já existe um caixa aberto");
        }

        var cashier = new Cashier
        {
            IsOpen = true,
            OpenedAt = DateTime.UtcNow,
            OpenedBy = userId,
            InitialAmount = initialAmount,
            CurrentAmount = initialAmount
        };

        var movement = new CashierMovement
        {
            Type = "OPEN",
            Amount = initialAmount,
            Observation = "Abertura de caixa",
            CashierId = cashier.Id
        };

        cashier.Movements.Add(movement);
        var createdCashier = await _cashierRepository.AddAsync(cashier, cancellationToken);

        return new CashierDto
        {
            Id = createdCashier.Id,
            IsOpen = createdCashier.IsOpen,
            OpenedAt = createdCashier.OpenedAt,
            OpenedBy = createdCashier.OpenedBy,
            InitialAmount = createdCashier.InitialAmount,
            CurrentAmount = createdCashier.CurrentAmount,
            TotalSales = createdCashier.TotalSales,
            TotalOrders = createdCashier.TotalOrders
        };
    }

    public async Task<CashierDto> CloseCashierAsync(CancellationToken cancellationToken = default)
    {
        var activeCashier = await _cashierRepository.FirstOrDefaultAsync(
            c => c.ClosedAt == null,
            cancellationToken
        );

        if (activeCashier == null)
        {
            throw new InvalidOperationException("Nenhum caixa está aberto");
        }

        activeCashier.IsOpen = false;
        activeCashier.ClosedAt = DateTime.UtcNow;

        var movement = new CashierMovement
        {
            Type = "CLOSE",
            Amount = activeCashier.CurrentAmount,
            Observation = "Fechamento de caixa",
            CashierId = activeCashier.Id
        };

        activeCashier.Movements.Add(movement);
        
        await _cashierRepository.UpdateAsync(activeCashier, cancellationToken);

        // Recarregar o caixa após atualizar para garantir dados corretos
        var updatedCashier = await _cashierRepository.GetByIdAsync(activeCashier.Id, cancellationToken);

        if (updatedCashier == null)
        {
            throw new InvalidOperationException("Erro ao recarregar caixa após fechamento");
        }

        // Garantir que IsOpen está sincronizado com ClosedAt
        var isOpen = updatedCashier.ClosedAt == null;

        return new CashierDto
        {
            Id = updatedCashier.Id,
            IsOpen = isOpen,
            OpenedAt = updatedCashier.OpenedAt,
            ClosedAt = updatedCashier.ClosedAt,
            OpenedBy = updatedCashier.OpenedBy,
            InitialAmount = updatedCashier.InitialAmount,
            CurrentAmount = updatedCashier.CurrentAmount,
            TotalSales = updatedCashier.TotalSales,
            TotalOrders = updatedCashier.TotalOrders
        };
    }

    public async Task<CashierMovementDto> ReceivePaymentAsync(Guid orderId, int amount, string paymentMethod, int? receivedAmount, CancellationToken cancellationToken = default)
    {
        var cashiers = await _cashierRepository.GetAllAsync(cancellationToken);
        var activeCashier = cashiers.FirstOrDefault(c => c.IsOpen);

        if (activeCashier == null)
        {
            throw new InvalidOperationException("Nenhum caixa está aberto");
        }

        if (receivedAmount.HasValue && receivedAmount.Value < amount)
        {
            throw new InvalidOperationException("Valor recebido não pode ser menor que o valor total");
        }

        int change = receivedAmount.HasValue ? receivedAmount.Value - amount : 0;

        var movement = new CashierMovement
        {
            Type = "SALE",
            Amount = amount,
            PaymentMethod = paymentMethod, // Salvar método de pagamento
            Observation = $"Pagamento do pedido {orderId} - Método: {paymentMethod}" + (change > 0 ? $" - Troco: {change}" : ""),
            CashierId = activeCashier.Id
        };

        activeCashier.Movements.Add(movement);
        activeCashier.CurrentAmount += amount;
        activeCashier.TotalSales += amount;
        activeCashier.TotalOrders += 1;

        await _cashierRepository.UpdateAsync(activeCashier, cancellationToken);

        // Atualizar daily_sales com o valor recebido (ignorar erro se tabela não existir)
        try
        {
            await _dailySalesService.UpsertDailySalesAsync(DateTime.UtcNow, amount, false, cancellationToken);
        }
        catch
        {
            // Ignorar erro se tabela daily_sales não existir ainda
        }

        return new CashierMovementDto
        {
            Id = movement.Id,
            Type = movement.Type,
            Amount = movement.Amount,
            Observation = movement.Observation,
            CashierId = movement.CashierId,
            CreatedAt = movement.CreatedAt,
            Change = change,
            PaymentMethod = paymentMethod
        };
    }

    public async Task<IEnumerable<CashierMovementDto>> GetSalesByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var cashiers = await _cashierRepository.GetAllAsync(cancellationToken);
        var targetDate = date.Date;

        var salesMovements = cashiers
            .SelectMany(c => c.Movements)
            .Where(m => m.Type == "SALE" && m.CreatedAt.Date == targetDate)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        return salesMovements.Select(m => new CashierMovementDto
        {
            Id = m.Id,
            Type = m.Type,
            Amount = m.Amount,
            Observation = m.Observation,
            CashierId = m.CashierId,
            CreatedAt = m.CreatedAt,
            Change = 0,
            PaymentMethod = m.PaymentMethod
        });
    }
}
