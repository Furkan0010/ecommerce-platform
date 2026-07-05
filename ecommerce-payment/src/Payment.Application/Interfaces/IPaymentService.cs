using Payment.Application.Common;
using Payment.Application.DTOs;

namespace Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentDto>> ProcessAsync(string buyerId, ProcessPaymentRequest request);
    Task<Result<PaymentDto>> GetByOrderNumberAsync(Guid orderNumber);
}
