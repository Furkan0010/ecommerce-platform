using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Common;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Application.Mapping;
using Payment.Domain.Interfaces;
using PaymentEntity = Payment.Domain.Entities.Payment;
using PaymentStatusEnum = Payment.Domain.Entities.PaymentStatus;

namespace Payment.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _gateway;
    private readonly IValidator<ProcessPaymentRequest> _validator;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IPaymentGateway gateway,
        IValidator<ProcessPaymentRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _gateway = gateway;
        _validator = validator;
    }

    public async Task<Result<PaymentDto>> ProcessAsync(string buyerId, ProcessPaymentRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<PaymentDto>.Failure(validation.Errors.Select(e => e.ErrorMessage));

        var payment = new PaymentEntity
        {
            OrderNumber = request.OrderNumber,
            BuyerId = buyerId,
            Amount = request.Amount,
            Status = PaymentStatusEnum.Pending
        };

        // Dış sağlayıcıya (sahte) ödeme isteği gönder.
        var gatewayResult = await _gateway.ChargeAsync(request.Amount, buyerId);

        if (gatewayResult.Approved)
        {
            payment.Status = PaymentStatusEnum.Succeeded;
            payment.ProviderReference = gatewayResult.Reference;
        }
        else
        {
            payment.Status = PaymentStatusEnum.Failed;
            payment.FailureReason = gatewayResult.DeclineReason;
        }

        await _unitOfWork.Repository<PaymentEntity>().AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Ödeme reddedilse bile bu "kayıt başarılı"dır; sonucu DTO'da Status taşır.
        return Result<PaymentDto>.Success(payment.ToDto());
    }

    public async Task<Result<PaymentDto>> GetByOrderNumberAsync(Guid orderNumber)
    {
        var payment = await _unitOfWork.Repository<PaymentEntity>()
            .Query().FirstOrDefaultAsync(p => p.OrderNumber == orderNumber);

        return payment is null
            ? Result<PaymentDto>.Failure($"Ödeme bulunamadı (Sipariş: {orderNumber}).")
            : Result<PaymentDto>.Success(payment.ToDto());
    }
}
