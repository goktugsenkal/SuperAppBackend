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
public class AccountController
    (UserManager<ApplicationUser> userManager, IEmailService emailService, IWebHostEnvironment environment) 
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<string>> GetMe()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        return Ok(user);
    }
    
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        return Ok();
    }

    private async Task SendResetPasswordEmailAsync()
    {
        
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(string email)
    {
        return Ok();
    }

    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please upload a valid image file.");

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest("Only image files are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("The image size should not exceed 5MB.");

        // Get the current user
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return Unauthorized();

        var userId = user.Id;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmss");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{userId}_avatar_{timestamp}{extension}";

        // Check if WebRootPath is null and ensure the directory exists
        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            // Initialize it to "wwwroot" directory if WebRootPath is null
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(webRootPath); // Create wwwroot if it doesn’t exist
        }

        // Set the avatars directory inside wwwroot/uploads/avatars
        var avatarDirectory = Path.Combine(webRootPath, "uploads", "avatars");
        Directory.CreateDirectory(avatarDirectory); // Ensure the avatars directory exists

        var filePath = Path.Combine(avatarDirectory, fileName);

        // Save the file to wwwroot/uploads/avatars
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update user's avatar URL (relative path)
        user.AvatarImageUrl = $"/uploads/avatars/{fileName}";
        await userManager.UpdateAsync(user);

        return Ok(new { message = "Profile picture uploaded successfully", avatarUrl = user.AvatarImageUrl });
    }

    [HttpGet("avatar/{fileName}")]
    public IActionResult GetAvatar(string fileName)
    {
        // Build the file path using WebRootPath
        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
        var avatarDirectory = Path.Combine(webRootPath, "uploads", "avatars");
        var filePath = Path.Combine(avatarDirectory, fileName);

        // Check if file exists
        if (!System.IO.File.Exists(filePath))
            return NotFound("Avatar not found");

        // Get the file's content type based on its extension
        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        string contentType = fileExtension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        // Serve the file
        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, contentType);
    }



}