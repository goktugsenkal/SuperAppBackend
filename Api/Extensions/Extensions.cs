using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

public static class Extensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Other service configurations...

        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwt:Key"]!)),
                    ValidIssuer = configuration["jwt:Issuer"],
                    ValidateIssuer = true,
                    ValidateAudience = false};
            });
        services.AddAuthorization();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition"); // Add any custom headers you want to expose
            });
        });
    }
}