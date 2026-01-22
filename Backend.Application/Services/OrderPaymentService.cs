using Backend.Application.DTOs.Order;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class OrderPaymentService
{
    private readonly IRepository<OrderPayment> _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Item> _itemRepository;

    public OrderPaymentService(
        IRepository<OrderPayment> paymentRepository,
        IOrderRepository orderRepository,
        IRepository<Item> itemRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
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
            // Calcular total apenas dos itens especificados
            var items = order.Items.Where(i => itemIds.Contains(i.Id)).ToList();
            orderTotal = items.Sum(i => i.Product.Price * i.Amount);
        }
        else
        {
            // Calcular total de todos os itens
            orderTotal = order.Items.Sum(i => i.Product.Price * i.Amount);
        }

        // Buscar pagamentos existentes
        var existingPayments = await _paymentRepository.FindAsync(p => p.OrderId == orderId, cancellationToken);
        var totalReceived = existingPayments.Sum(p => p.Amount);

        // Criar novo pagamento
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

        // Atualizar total recebido
        totalReceived += receivedAmount;
        var remainingAmount = orderTotal - totalReceived;

        // Se não há mais valor pendente, finalizar o pedido
        if (remainingAmount <= 0)
        {
            order.Status = true;
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

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
