using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.DTOs;
using Ordering.Application.Interfaces;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Ordering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Sipariş vermek için giriş gerekir.
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string BuyerId =>
        User.FindFirstValue(Claims.Subject)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Token içinde kullanıcı kimliği bulunamadı.");

    private string AccessToken
    {
        get
        {
            var header = Request.Headers.Authorization.ToString();
            return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? header["Bearer ".Length..]
                : header;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(BuyerId, request);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        return result.Succeeded ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var result = await _orderService.GetMyOrdersAsync(BuyerId);
        return Ok(result);
    }


    // YENİ: Gerçek checkout. Body boş; kalemler kullanıcının sepetinden çekilir.
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var result = await _orderService.CheckoutAsync(BuyerId, AccessToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }
}
