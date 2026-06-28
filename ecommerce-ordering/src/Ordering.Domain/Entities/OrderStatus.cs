namespace Ordering.Domain.Entities;

public enum OrderStatus
{
    Pending = 0,     // Oluşturuldu, saga henüz tamamlamadı
    Confirmed = 1,   // Stok + ödeme tamam
    Cancelled = 2    // Bir adım başarısız oldu, telafi edildi
}
