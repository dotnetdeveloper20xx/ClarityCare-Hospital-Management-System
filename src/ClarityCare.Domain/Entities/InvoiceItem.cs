namespace ClarityCare.Domain.Entities;

public class InvoiceItem
{
    public Guid InvoiceItemId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ServiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public ServiceCatalogue Service { get; set; } = null!;
}
