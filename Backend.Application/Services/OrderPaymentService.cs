using Backend.Application.DTOs.Order;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class OrderPaymentService
{
    private readonly IRepository<OrderPayment> _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly DailySalesService _dailySalesService;

    public OrderPaymentService(
        IRepository<OrderPayment> paymentRepository,
        IOrderRepository orderRepository,
        IRepository<Item> itemRepository,
        DailySalesService dailySalesService)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _dailySalesService = dailySalesService;
    }

    public async Task<ReceivePartialPaymentResponseDto> ReceivePartialPaymentAsync(
        Guid orderId,
        List<Guid>? itemIds,
        string paymentMethod,
        int receivedAmount,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        // Calcular total do pedido ou dos itens específicos
        int orderTotal;
        if (itemIds != null && itemIds.Any())
        {
            var items = order.Items.Where(i => itemIds.Contains(i.Id)).ToList();
            orderTotal = items.Sum(i => i.Product.Price * i.Amount);
        }
        else
        {
            orderTotal = order.Items.Sum(i => i.Product.Price * i.Amount);
        }

        // Buscar pagamentos existentes
        var existingPayments = await _paymentRepository.FindAsync(p => p.OrderId == orderId, cancellationToken);
        var totalReceived = existingPayments.Sum(p => p.Amount);

        var payment = new OrderPayment
        {
            OrderId = orderId,
            PaymentMethod = paymentMethod,
            Amount = receivedAmount,
            ReceivedAmount = receivedAmount,
            IsPartial = true,
            ItemIdsJson = itemIds != null && itemIds.Any()
                ? System.Text.Json.JsonSerializer.Serialize(itemIds)
                : null
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        // Marcar itens como pagos quando há pagamento parcial por itens específicos
        if (itemIds != null && itemIds.Any())
        {
            foreach (var itemId in itemIds)
            {
                var item = order.Items.FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    item.IsPaid = true;
                    await _itemRepository.UpdateAsync(item, cancellationToken);
                }
            }
        }

        totalReceived += receivedAmount;
        var remainingAmount = orderTotal - totalReceived;

        // Se não há mais valor pendente, finalizar o pedido
        if (remainingAmount <= 0)
        {
            // Marcar todos os itens restantes como pagos
            foreach (var item in order.Items.Where(i => !i.IsPaid))
            {
                item.IsPaid = true;
                await _itemRepository.UpdateAsync(item, cancellationToken);
            }
            
            order.Status = true;
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        // Atualizar daily_sales com o valor pago (não o total do pedido)
        await _dailySalesService.UpsertDailySalesAsync(DateTime.UtcNow, receivedAmount, false, cancellationToken);

        return new ReceivePartialPaymentResponseDto
        {
            success = true,
            remaining_amount = Math.Max(0, remainingAmount),
            total_received = totalReceived
        };
    }

    public async Task<OrderPaymentsResponseDto> GetOrderPaymentsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        var payments = await _paymentRepository.FindAsync(p => p.OrderId == orderId, cancellationToken);
        var orderTotal = order.Items.Sum(i => i.Product.Price * i.Amount);
        var totalReceived = payments.Sum(p => p.Amount);
        var remainingAmount = orderTotal - totalReceived;

        return new OrderPaymentsResponseDto
        {
            Payments = payments.Select(p => new OrderPaymentDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReceivedAmount = p.ReceivedAmount,
                IsPartial = p.IsPartial,
                ItemIds = !string.IsNullOrEmpty(p.ItemIdsJson) 
                    ? System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(p.ItemIdsJson) ?? new List<Guid>()
                    : new List<Guid>(),
                CreatedAt = p.CreatedAt
            }).ToList(),
            TotalReceived = totalReceived,
            RemainingAmount = Math.Max(0, remainingAmount),
            OrderTotal = orderTotal
        };
    }
}
