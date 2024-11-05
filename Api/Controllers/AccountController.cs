using System.Security.Claims;
using Api.Dtos;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("account")]
public class AccountController(UserManager<ApplicationUser> userManager, IEmailService emailService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FullName = registerDto.FullName
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (result.Succeeded)
        {
            // ?? should I send email verification link immediately ??
            return Ok("Kayıt başarılı.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<string>> GetMe()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        return Ok(user);
    }

    [HttpPost("request-email-verification")]
    public async Task<IActionResult> RequestEmailVerification()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return Unauthorized();
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink =
            Url.Action(nameof(VerifyEmail), "Account", new { userId = user.Id, token }, Request.Scheme);
        
        if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(confirmationLink))
        {
            return BadRequest();
        }
        
        await SendVerificationEmail(user.Email, user.FullName ?? "Trackin Kullanıcısı", confirmationLink);

        return Ok("Doğrulama e-postası yollandı.");
    }

    private async Task SendVerificationEmail(string email, string userName, string verificationLink)
    {
        var emailDto = new EmailDto
        {
            To = email,
            Subject = "Trackin E-posta Doğrulama",
            Body = $"""
                    <html>
                    <body style="background-color:#d9d9d9;padding:50px">
                        <p>Merhaba {userName},</p>
                        <p>Trackin'e kaydolduğunuz için teşekkürler! Lütfen aşağıdaki butona tıklayarak e-posta adresinizi doğrulayın:</p>
                        <a href='{verificationLink}' style='display:inline-block;padding:10px 20px;color:#ffffff;background-color:#1a73e8;border-radius:4px;text-decoration:none;'>E-postayı Doğrula</a>
                        <p>Eğer yukarıdaki buton çalışmıyorsa, aşağıdaki bağlantıyı kopyalayıp tarayıcınıza yapıştırabilirsiniz:</p>
                        <p>{verificationLink}</p>
                        <p>Saygılarımızla,<br>Trackin Ekibi</p>
                    </body>
                    </html>
                    """
        };

        await emailService.SendEmail(emailDto);
    }

    [AllowAnonymous]
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok("E-posta onaylandı.");
        }

        return BadRequest("Geçersiz ya da süresi geçmiş token.");
    }

}


/*

   [HttpPost("email")]
   public ActionResult SendTestEmail(EmailDto email)
   {
       _emailService.SendEmail(email);
       return Ok();
   }
*/