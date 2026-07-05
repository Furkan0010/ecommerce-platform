using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Payment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Ödeme hassas bir işlem; giriş gerekir.
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private string BuyerId =>
        User.FindFirstValue(Claims.Subject)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Token içinde kullanıcı kimliği bulunamadı.");

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request)
    {
        var result = await _paymentService.ProcessAsync(BuyerId, request);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{orderNumber:guid}")]
    public async Task<IActionResult> GetByOrderNumber(Guid orderNumber)
    {
        var result = await _paymentService.GetByOrderNumberAsync(orderNumber);
        return result.Succeeded ? Ok(result) : NotFound(result);
    }
}
