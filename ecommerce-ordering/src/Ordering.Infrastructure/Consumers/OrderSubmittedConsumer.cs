using ECommerce.Shared.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Ordering.Infrastructure.Consumers;

/// <summary>
/// GEÇİCİ demo dinleyici: OrderSubmitted olayı yayınlandığında onu yakalayıp
/// loglar. Amacı, yayınla -> RabbitMQ -> tüket döngüsünün çalıştığını görmek.
/// Faz 5'te bu mantık gerçek servislere (Catalog stok, Payment) taşınacak.
/// </summary>
public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly ILogger<OrderSubmittedConsumer> _logger;

    public OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "[EVENT] OrderSubmitted alındı -> OrderId: {OrderId}, Alıcı: {BuyerId}, Tutar: {Total}, Kalem sayısı: {Count}",
            msg.OrderId, msg.BuyerId, msg.Total, msg.Items.Count);
        return Task.CompletedTask;
    }
}
