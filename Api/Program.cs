using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Api.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddScoped<IFuelLogRepository, FuelLogRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Add security definition for Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token as follows: `Bearer {your token}`",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    // Add security requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<DataContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=>c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1"));
}

app.MapIdentityApi<ApplicationUser>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
