namespace Backend.Application.DTOs.Table;

public class TableDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
