using Payment.Domain.Common;

namespace Payment.Domain.Entities;

/// <summary>
/// Bir ödeme işleminin kaydı (transaction). Her sipariş için bir ödeme denemesi.
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderNumber { get; set; }          // hangi siparişe ait (Ordering'deki OrderNumber)
    public string BuyerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ProviderReference { get; set; } // sağlayıcının döndürdüğü referans
    public string? FailureReason { get; set; }
}
