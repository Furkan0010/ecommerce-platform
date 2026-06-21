using FluentValidation;
using Identity.Application.Common;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("connect")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterRequest> _validator;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterRequest> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    /// <summary>
    /// Yeni üye kaydı. POST /connect/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(Result.Failure(validation.Errors.Select(e => e.ErrorMessage)));

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(Result.Failure(result.Errors.Select(e => e.Description)));

        // Varsayılan olarak yeni üyeler "Customer" rolüne atanır.
        await _userManager.AddToRoleAsync(user, "Customer");

        return Ok(Result.Success());
    }
}
