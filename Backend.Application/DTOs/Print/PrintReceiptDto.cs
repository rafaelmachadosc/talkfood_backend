namespace Backend.Application.DTOs.Print;

public class PrintReceiptDto
{
    public string OrderId { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty; // "MESA" ou "BALCAO"
    public int? Table { get; set; }
    public string? CommandNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = new();
    public int Subtotal { get; set; } // em centavos
    public int Total { get; set; } // em centavos
    public string? PaymentMethod { get; set; }
    public int? ReceivedAmount { get; set; } // em centavos
    public int? Change { get; set; } // em centavos
    public string? EstablishmentName { get; set; }
    public string? EstablishmentAddress { get; set; }
    public string? EstablishmentPhone { get; set; }
    public string? EstablishmentCNPJ { get; set; }
}

public class ReceiptItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitPrice { get; set; } // em centavos
    public int TotalPrice { get; set; } // em centavos
}

public class PrinterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public bool IsActive { get; set; }
    public int PaperWidth { get; set; }
    public bool AutoPrint { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePrinterRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "THERMAL", "LASER", "INKJET"
    public string ConnectionType { get; set; } = string.Empty; // "USB", "NETWORK", "BLUETOOTH", "SERIAL"
    public string? ConnectionString { get; set; }
    public int PaperWidth { get; set; } = 80;
    public bool AutoPrint { get; set; } = false;
    public string? Settings { get; set; } // JSON string
}

public class UpdatePrinterRequestDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? ConnectionType { get; set; }
    public string? ConnectionString { get; set; }
    public bool? IsActive { get; set; }
    public int? PaperWidth { get; set; }
    public bool? AutoPrint { get; set; }
    public string? Settings { get; set; }
}

public class PrintRequestDto
{
    public Guid OrderId { get; set; }
    public Guid? PrinterId { get; set; } // opcional, se não informado usa impressora padrão
    public string ReceiptType { get; set; } = "ORDER"; // "ORDER", "PAYMENT", "CANCEL"
}

public class PrintResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public PrintReceiptDto? Receipt { get; set; }
    public string? RawData { get; set; } // dados formatados para impressão (ESC/POS, HTML, etc)
}
