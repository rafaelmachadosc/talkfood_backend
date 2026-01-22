using Backend.Application.DTOs.Analytics;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class AnalyticsService
{
    private readonly IRepository<OrderPayment> _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    public AnalyticsService(
        IRepository<OrderPayment> paymentRepository,
        IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<MetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Buscar todos os pedidos finalizados
        var allOrders = await _orderRepository.GetAllAsync(cancellationToken);
        var finishedOrders = allOrders.Where(o => o.Status).ToList();

        // Buscar todos os pagamentos
        var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);

        // Filtrar pagamentos de pedidos finalizados
        var finishedOrderIds = finishedOrders.Select(o => o.Id).ToHashSet();
        var finishedPayments = allPayments.Where(p => finishedOrderIds.Contains(p.OrderId)).ToList();

        // Calcular métricas por período
        var ordersToday = finishedOrders.Where(o => o.CreatedAt.Date == today).ToList();
        var ordersWeek = finishedOrders.Where(o => o.CreatedAt >= weekStart).ToList();
        var ordersMonth = finishedOrders.Where(o => o.CreatedAt >= monthStart).ToList();

        var paymentsToday = finishedPayments.Where(p => p.CreatedAt.Date == today).ToList();
        var paymentsWeek = finishedPayments.Where(p => p.CreatedAt >= weekStart).ToList();
        var paymentsMonth = finishedPayments.Where(p => p.CreatedAt >= monthStart).ToList();

        // Calcular totais
        var totalToday = paymentsToday.Sum(p => p.Amount);
        var totalWeek = paymentsWeek.Sum(p => p.Amount);
        var totalMonth = paymentsMonth.Sum(p => p.Amount);

        // Calcular média de ticket
        var averageTicket = ordersToday.Any() ? ordersToday.Sum(o => o.Items.Sum(i => i.Product.Price * i.Amount)) / ordersToday.Count : 0;

        // Calcular taxa de crescimento (comparar com semana anterior)
        var lastWeekStart = weekStart.AddDays(-7);
        var lastWeekEnd = weekStart;
        var lastWeekPayments = finishedPayments.Where(p => p.CreatedAt >= lastWeekStart && p.CreatedAt < lastWeekEnd).ToList();
        var lastWeekTotal = lastWeekPayments.Sum(p => p.Amount);
        var growthRate = lastWeekTotal > 0 ? ((totalWeek - lastWeekTotal) / (double)lastWeekTotal) * 100 : 0;

        // Agrupar pagamentos por método
        var paymentMethods = new PaymentMethodsDto();
        foreach (var payment in finishedPayments)
        {
            var method = payment.PaymentMethod?.ToUpper() ?? "";
            var amount = payment.Amount;

            switch (method)
            {
                case "DINHEIRO":
                    paymentMethods.DINHEIRO += amount;
                    break;
                case "PIX":
                    paymentMethods.PIX += amount;
                    break;
                case "CARTAO_CREDITO":
                case "CARTAO_CRÉDITO":
                case "CREDITO":
                case "CRÉDITO":
                    paymentMethods.CARTAO_CREDITO += amount;
                    break;
                case "CARTAO_DEBITO":
                case "CARTAO_DÉBITO":
                case "DEBITO":
                case "DÉBITO":
                    paymentMethods.CARTAO_DEBITO += amount;
                    break;
            }
        }

        return new MetricsDto
        {
            TotalToday = totalToday,
            TotalWeek = totalWeek,
            TotalMonth = totalMonth,
            OrdersToday = ordersToday.Count,
            OrdersWeek = ordersWeek.Count,
            OrdersMonth = ordersMonth.Count,
            AverageTicket = averageTicket,
            GrowthRate = growthRate,
            PaymentMethods = paymentMethods
        };
    }
}
