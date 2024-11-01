using System.Security.Claims;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(UserManager<ApplicationUser> userManager, IEmailService emailService) : ControllerBase
{
    private readonly IEmailService _emailService = emailService;
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<string>> GetMe()
    {
        // Get the user ID from the claims principal
        var user = await userManager.GetUserAsync(HttpContext.User);

        return Ok(user);
    }

    [HttpPost("email")]
    public ActionResult SendTestEmail(EmailDto email)
    {
        _emailService.SendEmail(email);
        return Ok();
    }
}