using System.Security.Claims;
using Api.Dtos;
using Core.Models;
using Core.Interfaces;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/fuel")]
public class FuelLogController
    (IFuelLogRepository fuelLogRepository, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedList<FuelLog>>> GetFuelLogs(int pageIndex = 1, int pageSize = 10)
    {
        if (pageIndex <= 0 || pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
        
        if (pageSize > 10) pageSize = 10;
        
        var userId = userManager.GetUserId(HttpContext.User);

        var fuelLogs = await fuelLogRepository
            .GetAllFuelLogsAsync(userId!, pageIndex, pageSize);
        
        return Ok(fuelLogs);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<FuelLog>> CreateFuelLog
        (CreateFuelLogDto? fuelLogForCreate)
    {
        if (fuelLogForCreate == null) return BadRequest("FuelLog data is required.");
        
        var user = await userManager.GetUserAsync(HttpContext.User);
        
        if(user == null) return Unauthorized();

        var fuelLog = new FuelLog
        {
            Notes = fuelLogForCreate.Notes,
            FuelEfficiency = fuelLogForCreate.FuelEfficiency,
            FuelStation = fuelLogForCreate.FuelStation,
            FuelType = fuelLogForCreate.FuelType,
            OdometerReading = fuelLogForCreate.OdometerReading,
            PaymentMethod = fuelLogForCreate.PaymentMethod,
            TotalCost = fuelLogForCreate.TotalCost,
            ApplicationUserId = user.Id,
            FillUpDate = fuelLogForCreate.FillUpDate,
            FuelVolumeCalculated = fuelLogForCreate.FuelVolumeCalculated,
            FuelVolumeFilled = fuelLogForCreate.FuelVolumeFilled,
            IsTankFull = fuelLogForCreate.IsTankFull,
            PricePerUnit = fuelLogForCreate.PricePerUnit,
            VehiclePlateNumber = fuelLogForCreate.VehiclePlateNumber
        };
        
        fuelLogRepository.CreateFuelLog(fuelLog);
        return Ok(fuelLog);
    }
}