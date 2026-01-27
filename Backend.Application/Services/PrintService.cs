using Backend.Application.DTOs.Order;
using Backend.Application.DTOs.Print;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class PrintService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Printer> _printerRepository;
    private readonly OrderPaymentService _paymentService;

    public PrintService(
        IOrderRepository orderRepository,
        IRepository<Printer> printerRepository,
        OrderPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _printerRepository = printerRepository;
        _paymentService = paymentService;
    }

    public async Task<PrintReceiptDto> GenerateReceiptAsync(Guid orderId, string receiptType, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException("Pedido não encontrado");
        }

        var items = order.Items?.Where(i => !i.IsPaid || receiptType == "PAYMENT").ToList() ?? new List<Item>();
        
        var receiptItems = items.Select(i => new ReceiptItemDto
        {
            ProductName = i.Product?.Name ?? "Produto não encontrado",
            Quantity = i.Amount,
            UnitPrice = i.Product?.Price ?? 0,
            TotalPrice = (i.Product?.Price ?? 0) * i.Amount
        }).ToList();

        var subtotal = receiptItems.Sum(i => i.TotalPrice);
        var total = subtotal;

        // Buscar informações de pagamento se for cupom de pagamento
        int? receivedAmount = null;
        int? change = null;
        string? paymentMethod = null;

        if (receiptType == "PAYMENT" && order.Status)
        {
            var payments = await _paymentService.GetOrderPaymentsAsync(orderId, cancellationToken);
            receivedAmount = payments.TotalReceived;
            change = payments.RemainingAmount < 0 ? Math.Abs(payments.RemainingAmount) : null;
            
            if (payments.Payments.Any())
            {
                paymentMethod = payments.Payments.First().PaymentMethod;
            }
        }

        return new PrintReceiptDto
        {
            OrderId = order.Id.ToString(),
            OrderType = order.OrderType == Domain.Enums.OrderType.Mesa ? "MESA" : "BALCAO",
            Table = order.Table,
            CommandNumber = order.CommandNumber,
            CustomerName = order.Name,
            Phone = order.Phone,
            CreatedAt = order.CreatedAt,
            Items = receiptItems,
            Subtotal = subtotal,
            Total = total,
            PaymentMethod = paymentMethod,
            ReceivedAmount = receivedAmount,
            Change = change,
            EstablishmentName = "Estabelecimento", // TODO: buscar de configurações
            EstablishmentAddress = null, // TODO: buscar de configurações
            EstablishmentPhone = null, // TODO: buscar de configurações
            EstablishmentCNPJ = null // TODO: buscar de configurações
        };
    }

    public Task<string> FormatReceiptForPrintAsync(PrintReceiptDto receipt, Printer? printer, CancellationToken cancellationToken = default)
    {
        var paperWidth = printer?.PaperWidth ?? 80;
        // Sempre retorna formato térmico (texto simples) - nunca HTML
        // O HTML pode ser gerado no frontend se necessário
        return Task.FromResult(FormatThermalReceipt(receipt, paperWidth));
    }

    private string FormatThermalReceipt(PrintReceiptDto receipt, int paperWidth)
    {
        var lines = new List<string>();
        var charsPerLine = paperWidth == 80 ? 48 : 32; // 80mm = 48 chars, 58mm = 32 chars

        // Cabeçalho
        lines.Add(CenterText("=".PadRight(charsPerLine, '='), charsPerLine));
        if (!string.IsNullOrEmpty(receipt.EstablishmentName))
        {
            lines.Add(CenterText(receipt.EstablishmentName, charsPerLine));
        }
        if (!string.IsNullOrEmpty(receipt.EstablishmentAddress))
        {
            lines.Add(CenterText(receipt.EstablishmentAddress, charsPerLine));
        }
        if (!string.IsNullOrEmpty(receipt.EstablishmentPhone))
        {
            lines.Add(CenterText(receipt.EstablishmentPhone, charsPerLine));
        }
        if (!string.IsNullOrEmpty(receipt.EstablishmentCNPJ))
        {
            lines.Add(CenterText($"CNPJ: {receipt.EstablishmentCNPJ}", charsPerLine));
        }
        lines.Add(CenterText("=".PadRight(charsPerLine, '='), charsPerLine));
        lines.Add("");

        // Informações do pedido
        lines.Add($"Pedido: {receipt.OrderId.Substring(0, 8)}");
        lines.Add($"Data: {receipt.CreatedAt:dd/MM/yyyy HH:mm:ss}");
        lines.Add("");

        if (receipt.OrderType == "MESA")
        {
            lines.Add($"Mesa: {receipt.Table}");
            if (!string.IsNullOrEmpty(receipt.CommandNumber))
            {
                lines.Add($"Comanda: {receipt.CommandNumber}");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(receipt.CustomerName))
            {
                lines.Add($"Cliente: {receipt.CustomerName}");
            }
            if (!string.IsNullOrEmpty(receipt.Phone))
            {
                lines.Add($"Telefone: {receipt.Phone}");
            }
        }
        lines.Add("");
        lines.Add(CenterText("-".PadRight(charsPerLine, '-'), charsPerLine));

        // Itens
        lines.Add("ITEM                    QTD  UNIT    TOTAL");
        lines.Add(CenterText("-".PadRight(charsPerLine, '-'), charsPerLine));

        foreach (var item in receipt.Items)
        {
            var productName = TruncateText(item.ProductName, 20);
            var quantity = item.Quantity.ToString().PadLeft(3);
            var unitPrice = FormatCurrency(item.UnitPrice).PadLeft(8);
            var totalPrice = FormatCurrency(item.TotalPrice).PadLeft(10);
            
            lines.Add($"{productName.PadRight(20)} {quantity} {unitPrice} {totalPrice}");
        }

        lines.Add(CenterText("-".PadRight(charsPerLine, '-'), charsPerLine));

        // Totais
        lines.Add($"SUBTOTAL: {FormatCurrency(receipt.Subtotal).PadLeft(charsPerLine - 9)}");
        lines.Add($"TOTAL: {FormatCurrency(receipt.Total).PadLeft(charsPerLine - 6)}");

        if (receipt.ReceivedAmount.HasValue)
        {
            lines.Add($"RECEBIDO: {FormatCurrency(receipt.ReceivedAmount.Value).PadLeft(charsPerLine - 9)}");
        }

        if (receipt.Change.HasValue && receipt.Change.Value > 0)
        {
            lines.Add($"TROCO: {FormatCurrency(receipt.Change.Value).PadLeft(charsPerLine - 6)}");
        }

        if (!string.IsNullOrEmpty(receipt.PaymentMethod))
        {
            lines.Add($"FORMA DE PAGAMENTO: {receipt.PaymentMethod}");
        }

        lines.Add("");
        lines.Add(CenterText("=".PadRight(charsPerLine, '='), charsPerLine));
        lines.Add(CenterText("OBRIGADO PELA PREFERENCIA!", charsPerLine));
        lines.Add(CenterText("=".PadRight(charsPerLine, '='), charsPerLine));
        lines.Add("");
        lines.Add(""); // Espaço para cortar

        return string.Join("\n", lines);
    }

    private string FormatHtmlReceipt(PrintReceiptDto receipt)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: monospace; font-size: 12px; margin: 0; padding: 10px; }}
        .header {{ text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; margin-bottom: 10px; }}
        .item {{ display: flex; justify-content: space-between; margin: 5px 0; }}
        .total {{ border-top: 1px solid #000; padding-top: 10px; margin-top: 10px; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 20px; border-top: 2px solid #000; padding-top: 10px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>{receipt.EstablishmentName ?? "Estabelecimento"}</h2>
        {(!string.IsNullOrEmpty(receipt.EstablishmentAddress) ? $"<p>{receipt.EstablishmentAddress}</p>" : "")}
        {(!string.IsNullOrEmpty(receipt.EstablishmentPhone) ? $"<p>{receipt.EstablishmentPhone}</p>" : "")}
        {(!string.IsNullOrEmpty(receipt.EstablishmentCNPJ) ? $"<p>CNPJ: {receipt.EstablishmentCNPJ}</p>" : "")}
    </div>
    
    <p><strong>Pedido:</strong> {receipt.OrderId.Substring(0, 8)}</p>
    <p><strong>Data:</strong> {receipt.CreatedAt:dd/MM/yyyy HH:mm:ss}</p>
    
    {(receipt.OrderType == "MESA" ? $"<p><strong>Mesa:</strong> {receipt.Table}</p>" : "")}
    {(!string.IsNullOrEmpty(receipt.CommandNumber) ? $"<p><strong>Comanda:</strong> {receipt.CommandNumber}</p>" : "")}
    {(!string.IsNullOrEmpty(receipt.CustomerName) ? $"<p><strong>Cliente:</strong> {receipt.CustomerName}</p>" : "")}
    {(!string.IsNullOrEmpty(receipt.Phone) ? $"<p><strong>Telefone:</strong> {receipt.Phone}</p>" : "")}
    
    <hr>
    
    <table style='width: 100%;'>
        <tr>
            <th style='text-align: left;'>Item</th>
            <th style='text-align: center;'>Qtd</th>
            <th style='text-align: right;'>Unit</th>
            <th style='text-align: right;'>Total</th>
        </tr>
";

        foreach (var item in receipt.Items)
        {
            html += $@"
        <tr>
            <td>{item.ProductName}</td>
            <td style='text-align: center;'>{item.Quantity}</td>
            <td style='text-align: right;'>{FormatCurrency(item.UnitPrice)}</td>
            <td style='text-align: right;'>{FormatCurrency(item.TotalPrice)}</td>
        </tr>
";
        }

        html += $@"
    </table>
    
    <div class='total'>
        <p>SUBTOTAL: {FormatCurrency(receipt.Subtotal)}</p>
        <p>TOTAL: {FormatCurrency(receipt.Total)}</p>
        {(receipt.ReceivedAmount.HasValue ? $"<p>RECEBIDO: {FormatCurrency(receipt.ReceivedAmount.Value)}</p>" : "")}
        {(receipt.Change.HasValue && receipt.Change.Value > 0 ? $"<p>TROCO: {FormatCurrency(receipt.Change.Value)}</p>" : "")}
        {(!string.IsNullOrEmpty(receipt.PaymentMethod) ? $"<p>FORMA DE PAGAMENTO: {receipt.PaymentMethod}</p>" : "")}
    </div>
    
    <div class='footer'>
        <p>OBRIGADO PELA PREFERÊNCIA!</p>
    </div>
</body>
</html>";

        return html;
    }

    private string CenterText(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        var padding = (width - text.Length) / 2;
        return text.PadLeft(padding + text.Length).PadRight(width);
    }

    private string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 3) + "...";
    }

    private string FormatCurrency(int cents)
    {
        return $"R$ {(cents / 100.0):F2}";
    }

    public async Task<IEnumerable<PrinterDto>> GetAllPrintersAsync(CancellationToken cancellationToken = default)
    {
        var printers = await _printerRepository.GetAllAsync(cancellationToken);
        return printers.Select(p => new PrinterDto
        {
            Id = p.Id,
            Name = p.Name,
            Type = p.Type,
            ConnectionType = p.ConnectionType,
            ConnectionString = p.ConnectionString,
            IsActive = p.IsActive,
            PaperWidth = p.PaperWidth,
            AutoPrint = p.AutoPrint,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<PrinterDto> CreatePrinterAsync(CreatePrinterRequestDto request, CancellationToken cancellationToken = default)
    {
        var printer = new Printer
        {
            Name = request.Name,
            Type = request.Type,
            ConnectionType = request.ConnectionType,
            ConnectionString = request.ConnectionString,
            PaperWidth = request.PaperWidth,
            AutoPrint = request.AutoPrint,
            Settings = request.Settings,
            IsActive = true
        };

        var created = await _printerRepository.AddAsync(printer, cancellationToken);
        
        return new PrinterDto
        {
            Id = created.Id,
            Name = created.Name,
            Type = created.Type,
            ConnectionType = created.ConnectionType,
            ConnectionString = created.ConnectionString,
            IsActive = created.IsActive,
            PaperWidth = created.PaperWidth,
            AutoPrint = created.AutoPrint,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<PrinterDto> UpdatePrinterAsync(UpdatePrinterRequestDto request, CancellationToken cancellationToken = default)
    {
        var printer = await _printerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (printer == null)
        {
            throw new KeyNotFoundException("Impressora não encontrada");
        }

        if (!string.IsNullOrEmpty(request.Name)) printer.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Type)) printer.Type = request.Type;
        if (!string.IsNullOrEmpty(request.ConnectionType)) printer.ConnectionType = request.ConnectionType;
        if (request.ConnectionString != null) printer.ConnectionString = request.ConnectionString;
        if (request.IsActive.HasValue) printer.IsActive = request.IsActive.Value;
        if (request.PaperWidth.HasValue) printer.PaperWidth = request.PaperWidth.Value;
        if (request.AutoPrint.HasValue) printer.AutoPrint = request.AutoPrint.Value;
        if (request.Settings != null) printer.Settings = request.Settings;

        await _printerRepository.UpdateAsync(printer, cancellationToken);

        var updated = await _printerRepository.GetByIdAsync(request.Id, cancellationToken);
        return new PrinterDto
        {
            Id = updated!.Id,
            Name = updated.Name,
            Type = updated.Type,
            ConnectionType = updated.ConnectionType,
            ConnectionString = updated.ConnectionString,
            IsActive = updated.IsActive,
            PaperWidth = updated.PaperWidth,
            AutoPrint = updated.AutoPrint,
            CreatedAt = updated.CreatedAt
        };
    }

    public async Task DeletePrinterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var printer = await _printerRepository.GetByIdAsync(id, cancellationToken);
        if (printer == null)
        {
            throw new KeyNotFoundException("Impressora não encontrada");
        }

        await _printerRepository.DeleteAsync(printer, cancellationToken);
    }
}
