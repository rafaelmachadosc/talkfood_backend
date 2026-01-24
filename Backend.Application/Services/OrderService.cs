using Backend.Application.DTOs.Order;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRepository<Item> _itemRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IRepository<Product> productRepository,
        ITableRepository tableRepository,
        IRepository<Item> itemRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _tableRepository = tableRepository;
        _itemRepository = itemRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(int? table, string? name, string? phone, int? commandNumber, OrderType orderType, CancellationToken cancellationToken = default)
    {
        Guid? tableId = null;
        if (table.HasValue)
        {
            var tableEntity = await _tableRepository.GetByNumberAsync(table.Value, cancellationToken);
            tableId = tableEntity?.Id;
        }

        var order = new Order
        {
            Table = table,
            TableId = tableId,
            Name = name,
            Phone = phone,
            CommandNumber = commandNumber,
            OrderType = orderType,
            Draft = true,
            Status = false
        };

        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);
        return MapToDto(createdOrder);
    }

    public async Task<OrderDto> AddItemAsync(Guid orderId, Guid productId, int amount, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException("Produto não encontrado");
        }

        var item = new Item
        {
            OrderId = orderId,
            ProductId = productId,
            Amount = amount
        };

        await _itemRepository.AddAsync(item, cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task RemoveItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _itemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null)
        {
            throw new KeyNotFoundException("Item não encontrado");
        }

        await _itemRepository.DeleteAsync(item, cancellationToken);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetDraftOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetDraftOrdersAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetNonDraftOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetNonDraftOrdersAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByTableAsync(int table, string? phone, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByTableAsync(table, phone, cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        return MapToDto(order);
    }

    public async Task<OrderDto> SendOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        order.Draft = false;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Recarregar o pedido completo com itens após atualizar
        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task<OrderDto> FinishOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        order.Status = true;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Recarregar o pedido completo com itens após atualizar
        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task DeleteOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        await _orderRepository.DeleteAsync(order, cancellationToken);
    }

    public async Task<OrderDto> MarkAsViewedAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        order.Viewed = true;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task<OrderDto> AddMultipleItemsAsync(Guid orderId, List<(Guid productId, int amount)> items, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        if (!order.Draft)
        {
            throw new InvalidOperationException("Não é possível adicionar itens a um pedido que já foi enviado para produção");
        }

        foreach (var (productId, amount) in items)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException($"Produto {productId} não encontrado");
            }

            var item = new Item
            {
                OrderId = orderId,
                ProductId = productId,
                Amount = amount
            };

            await _itemRepository.AddAsync(item, cancellationToken);
        }

        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task<OrderDto> UpdateCommandNumberAsync(Guid orderId, int? commandNumber, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        order.CommandNumber = commandNumber;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task<OrderDto> UpdateOrderInfoAsync(Guid orderId, string? name, int? commandNumber, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        if (name != null && name.Trim() != "") 
        {
            order.Name = name.Trim();
        }
        else if (name == "")
        {
            order.Name = null;
        }
        
        if (commandNumber.HasValue) 
        {
            order.CommandNumber = commandNumber.Value;
        }
        else if (commandNumber == null && name == null)
        {
            // Se ambos são null, não fazer nada ou resetar
            order.CommandNumber = null;
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return MapToDto(updatedOrder!);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCommandOrNameAsync(int? commandNumber, string? name, CancellationToken cancellationToken = default)
    {
        var allOrders = await _orderRepository.GetAllAsync(cancellationToken);
        
        if (commandNumber.HasValue)
        {
            allOrders = allOrders.Where(o => o.CommandNumber == commandNumber.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            allOrders = allOrders.Where(o => o.Name != null && o.Name.ToLower().Contains(name.ToLower()));
        }

        return allOrders.Select(MapToDto);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            Table = order.Table,
            TableId = order.TableId,
            Status = order.Status,
            Draft = order.Draft,
            Name = order.Name,
            Phone = order.Phone,
            CommandNumber = order.CommandNumber,
            OrderType = order.OrderType,
            orderType = order.OrderType == OrderType.Mesa ? "MESA" : "BALCAO",
            Viewed = order.Viewed,
            Items = order.Items.Where(i => !i.IsPaid).Select(i => new ItemDto
            {
                Id = i.Id,
                Amount = i.Amount,
                ProductId = i.ProductId,
                Product = i.Product != null ? new ProductDto
                {
                    Id = i.Product.Id,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    Description = i.Product.Description
                } : null
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
