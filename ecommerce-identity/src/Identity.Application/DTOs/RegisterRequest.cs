namespace Identity.Application.DTOs;

/// <summary>
/// Yeni üye kaydı isteği. Giriş (login) işlemi ayrı bir DTO gerektirmez;
/// OAuth2 standardı gereği token uç noktası (/connect/token) form alanlarıyla
/// (username, password, grant_type, scope) çalışır.
/// </summary>
public record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string? FullName);
