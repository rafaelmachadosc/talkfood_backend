namespace Backend.Domain.Entities;

public class Printer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "THERMAL", "LASER", "INKJET"
    public string ConnectionType { get; set; } = string.Empty; // "USB", "NETWORK", "BLUETOOTH", "SERIAL"
    public string? ConnectionString { get; set; } // IP, porta, nome da impressora, etc.
    public bool IsActive { get; set; } = true;
    public int PaperWidth { get; set; } = 80; // largura do papel em mm (padrão 80mm para térmica)
    public string? Settings { get; set; } // JSON com configurações específicas
    public bool AutoPrint { get; set; } = false; // imprimir automaticamente ao finalizar pedido
}
