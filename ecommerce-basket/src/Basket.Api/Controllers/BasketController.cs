using System.Security.Claims;
using Basket.Application.DTOs;
using Basket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Basket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Sepet, yalnızca giriş yapmış kullanıcıya aittir.
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    // Sepet sahibi: token'daki "sub" (kullanıcı id) claim'i.
    private string BuyerId =>
        User.FindFirstValue(Claims.Subject)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Token içinde kullanıcı kimliği bulunamadı.");

    [HttpGet]
    public async Task<IActionResult> GetMyBasket()
    {
        var result = await _basketService.GetBasketAsync(BuyerId);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request)
    {
        var result = await _basketService.AddItemAsync(BuyerId, request);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var result = await _basketService.RemoveItemAsync(BuyerId, productId);
        return result.Succeeded ? Ok(result) : NotFound(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var result = await _basketService.ClearAsync(BuyerId);
        return Ok(result);
    }
}
