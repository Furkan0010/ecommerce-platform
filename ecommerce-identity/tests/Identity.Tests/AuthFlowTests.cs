using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Identity.Tests;

/// <summary>
/// BlogPlatform'daki WebApplicationFactory tabanlı entegrasyon testi kurgusunun
/// aynısı. NOT: Bu test gerçek bir PostgreSQL bağlantısı bekler (Testcontainers
/// ile ayağa kaldırılması önerilir). Burada yapı/iskelet gösterilmektedir.
/// </summary>
public class AuthFlowTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_Then_GetToken_Should_Return_AccessToken()
    {
        // 1) Üye ol
        var register = await _client.PostAsJsonAsync("/connect/register", new
        {
            userName = "testuser",
            email = "[email protected]",
            password = "Test123!",
            fullName = "Test Kullanıcı"
        });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        // 2) Token al (password grant)
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "testuser",
            ["password"] = "Test123!",
            ["scope"] = "ecommerce-api offline_access"
        });

        var tokenResponse = await _client.PostAsync("/connect/token", form);
        tokenResponse.EnsureSuccessStatusCode();

        var payload = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(payload?.AccessToken));
    }

    private record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken);
}
