using Backend.Application.DTOs.Cashier;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class CashierService
{
    private readonly IRepository<Cashier> _cashierRepository;

    public CashierService(IRepository<Cashier> cashierRepository)
    {
        _cashierRepository = cashierRepository;
    }

    public async Task<CashierDto> GetCashierStatusAsync(CancellationToken cancellationToken = default)
    {
        var cashiers = await _cashierRepository.GetAllAsync(cancellationToken);
        var activeCashier = cashiers.FirstOrDefault(c => c.IsOpen);

        if (activeCashier == null)
        {
            return new CashierDto
            {
                IsOpen = false
            };
        }

        return new CashierDto
        {
            Id = activeCashier.Id,
            IsOpen = activeCashier.IsOpen,
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
        var cashiers = await _cashierRepository.GetAllAsync(cancellationToken);
        var activeCashier = cashiers.FirstOrDefault(c => c.IsOpen);

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
        var cashiers = await _cashierRepository.GetAllAsync(cancellationToken);
        var activeCashier = cashiers.FirstOrDefault(c => c.IsOpen);

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

        return new CashierDto
        {
            Id = activeCashier.Id,
            IsOpen = activeCashier.IsOpen,
            OpenedAt = activeCashier.OpenedAt,
            ClosedAt = activeCashier.ClosedAt,
            OpenedBy = activeCashier.OpenedBy,
            InitialAmount = activeCashier.InitialAmount,
            CurrentAmount = activeCashier.CurrentAmount,
            TotalSales = activeCashier.TotalSales,
            TotalOrders = activeCashier.TotalOrders
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

        return new CashierMovementDto
        {
            Id = movement.Id,
            Type = movement.Type,
            Amount = movement.Amount,
            Observation = movement.Observation,
            CashierId = movement.CashierId,
            CreatedAt = movement.CreatedAt,
            Change = change
        };
    }
}
