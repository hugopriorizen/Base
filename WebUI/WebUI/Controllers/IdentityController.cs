using System.Security.Claims;
using Application.Identity.Commands.LoginUser;
using Application.Identity.Commands.RegisterUser;
using Application.Identity.DTOs;
using Application.Identity.Queries.GetUserById;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityController(
        IMediator mediator,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager
    )
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(new RegisterUserCommand(registerDto));

        if (result.Succeeded)
        {
            return Ok(new { Message = result.Message });
        }

        return BadRequest(new { Message = result.Message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(new LoginUserCommand(loginDto));

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                };

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = loginDto.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                return Ok(new { Message = result.Message });
            }
        }

        return Unauthorized(new { Message = result.Message });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { Message = "Logout successful" });
    }

    [HttpGet("currentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _mediator.Send(new GetUserByIdQuery(userId));
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(resetDto.Email);
        if (user == null)
        {
            // For security, don't reveal that the user does not exist
            return Ok(
                new
                {
                    Message = "If your email is registered, a password reset link has been sent.",
                }
            );
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            resetDto.Token,
            resetDto.NewPassword
        );
        if (result.Succeeded)
        {
            return Ok(new { Message = "Password has been reset successfully." });
        }

        return BadRequest(
            new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) }
        );
    }

    [HttpPost("changepassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto changeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || userId != changeDto.UserId)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            changeDto.CurrentPassword,
            changeDto.NewPassword
        );
        if (result.Succeeded)
        {
            return Ok(new { Message = "Password changed successfully." });
        }

        return BadRequest(
            new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) }
        );
    }

    [HttpPut("updateprofile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || userId != updateDto.Id)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.Address = updateDto.Address;

        // Only allow email change if it doesn't conflict with another user
        if (user.Email != updateDto.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
            if (existingUser != null && existingUser.Id != userId)
            {
                return BadRequest(new { Message = "Email is already in use." });
            }

            user.Email = updateDto.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(updateDto.Email);
            user.EmailConfirmed = false; // Require re-confirmation
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // If email changed, send confirmation email here
            // ...

            return Ok(new { Message = "Profile updated successfully." });
        }

        return BadRequest(
            new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) }
        );
    }

    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { Message = "Email is required." });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // For security, don't reveal that the user does not exist
            return Ok(
                new
                {
                    Message = "If your email is registered, a password reset link has been sent.",
                }
            );
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // In a real application, send an email with the token
        // EmailService.SendPasswordResetEmail(user.Email, token);

        return Ok(
            new { Message = "If your email is registered, a password reset link has been sent." }
        );
    }
}
