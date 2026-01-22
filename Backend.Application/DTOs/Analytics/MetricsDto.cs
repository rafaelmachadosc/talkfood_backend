namespace Backend.Application.DTOs.Analytics;

public class MetricsDto
{
    public int TotalToday { get; set; } = 0; // em centavos
    public int TotalWeek { get; set; } = 0; // em centavos
    public int TotalMonth { get; set; } = 0; // em centavos
    public int OrdersToday { get; set; } = 0;
    public int OrdersWeek { get; set; } = 0;
    public int OrdersMonth { get; set; } = 0;
    public int AverageTicket { get; set; } = 0; // em centavos
    public double GrowthRate { get; set; } = 0.0; // porcentagem
    public PaymentMethodsDto PaymentMethods { get; set; } = new();
}

public class PaymentMethodsDto
{
    public int DINHEIRO { get; set; } = 0; // em centavos
    public int PIX { get; set; } = 0; // em centavos
    public int CARTAO_CREDITO { get; set; } = 0; // em centavos
    public int CARTAO_DEBITO { get; set; } = 0; // em centavos
}
