using System.Collections.Immutable;
using System.Security.Claims;
using Identity.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Api.Controllers;

/// <summary>
/// OAuth2 / OIDC token uç noktası. Tüm servislerin doğrulayacağı JWT'leri
/// burası üretir. Bu ilk sürümde iki akışı destekliyoruz:
///   - password        : kullanıcı adı + parola ile token (frontend yokken test için ideal)
///   - refresh_token   : süresi dolan access token'ı yeniden almak için
/// İleride frontend geldiğinde Authorization Code + PKCE buraya eklenir;
/// diğer servislerin token doğrulaması ise hangi akışla üretildiğinden bağımsızdır.
/// </summary>
[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthorizationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict isteği çözümlenemedi.");

        if (request.IsPasswordGrantType())
            return await HandlePasswordGrantAsync(request);

        if (request.IsRefreshTokenGrantType())
            return await HandleRefreshTokenGrantAsync();

        throw new NotImplementedException("Desteklenmeyen grant türü.");
    }

    private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username!);
        if (user is null)
            return Forbidden("Kullanıcı adı veya parola hatalı.");

        var check = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
        if (!check.Succeeded)
            return Forbidden("Kullanıcı adı veya parola hatalı.");

        var principal = await CreatePrincipalAsync(user, request.GetScopes());
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandleRefreshTokenGrantAsync()
    {
        // OpenIddict, refresh token'ı zaten doğruladı; ilgili principal'ı alıyoruz.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var user = result.Principal is not null ? await _userManager.GetUserAsync(result.Principal) : null;
        if (user is null)
            return Forbidden("Refresh token geçersiz veya kullanıcı bulunamadı.");

        var principal = await CreatePrincipalAsync(user, result.Principal!.GetScopes());
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Kullanıcı için token'a gömülecek claim'leri taşıyan principal'ı kurar.
    /// </summary>
    private async Task<ClaimsPrincipal> CreatePrincipalAsync(ApplicationUser user, ImmutableArray<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

        identity.SetClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)]);

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);
        principal.SetResources("ecommerce-api");

        // Her claim'in hangi token'a (access / identity) gireceğini belirliyoruz.
        foreach (var claim in principal.Claims)
            claim.SetDestinations(GetDestinations(claim, principal));

        return principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;
                yield break;

            // Güvenlik damgası asla token'a gömülmez.
            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }

    private ForbidResult Forbidden(string description)
    {
        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
        });

        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
