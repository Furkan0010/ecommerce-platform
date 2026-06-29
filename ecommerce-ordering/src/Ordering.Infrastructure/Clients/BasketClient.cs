using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ordering.Application.Interfaces;
 
namespace Ordering.Infrastructure.Clients;
 
/// <summary>
/// IBasketClient'in HTTP implementasyonu. Basket servisinin GET /api/basket ucuna
/// gider. Bu uç [Authorize]'lı olduğu için kullanıcının token'ını taşır (token forwarding).
/// HttpClient, DI tarafında Polly dayanıklılık politikalarıyla sarılır.
/// </summary>
public class BasketClient : IBasketClient
{
    private readonly HttpClient _httpClient;
 
    public BasketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
 
    public async Task<BasketInfo?> GetBasketAsync(string buyerId, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/basket");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
 
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;
 
        // Basket, Result<BasketDto> zarfı döner: { succeeded, errors, data: { buyerId, items[], total } }
        var payload = await response.Content.ReadFromJsonAsync<BasketResultResponse>();
        if (payload?.Data?.Items is null)
            return null;
 
        var lines = payload.Data.Items
            .Select(i => new BasketLine(i.ProductId, i.ProductName, i.Price, i.Quantity))
            .ToList();
 
        return new BasketInfo(lines);
    }
 
    private record BasketResultResponse(BasketData? Data);
    private record BasketData(List<BasketItemData> Items);
    private record BasketItemData(int ProductId, string ProductName, decimal Price, int Quantity);
}