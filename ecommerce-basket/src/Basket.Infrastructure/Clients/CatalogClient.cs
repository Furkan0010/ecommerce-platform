using System.Net;
using System.Net.Http.Json;
using Basket.Application.Interfaces;

namespace Basket.Infrastructure.Clients;

/// <summary>
/// ICatalogClient'in HTTP implementasyonu. Catalog servisinin
/// GET /api/products/{id} ucuna gider ve Result&lt;ProductDto&gt; zarfını çözer.
/// HttpClient, DI tarafında Polly dayanıklılık politikalarıyla sarılır.
/// </summary>
public class CatalogClient : ICatalogClient
{
    private readonly HttpClient _httpClient;

    public CatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductInfo?> GetProductAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"/api/products/{productId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CatalogResultResponse>();
        if (payload?.Data is null)
            return null;

        return new ProductInfo(
            payload.Data.Id,
            payload.Data.Name,
            payload.Data.Price,
            payload.Data.StockQuantity);
    }

    // Catalog'un döndürdüğü Result<ProductDto> zarfının ihtiyacımız olan kısmı.
    private record CatalogResultResponse(ProductData? Data);
    private record ProductData(int Id, string Name, decimal Price, int StockQuantity);
}
