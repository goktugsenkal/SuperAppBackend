using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class FuelLogRepository(DataContext context) : IFuelLogRepository
{
    public async Task<PagedList<FuelLog>> GetAllFuelLogsAsync(string userId, int pageNumber, int pageSize)
    {
        var query = context.FuelLogs
            .Where(fl => fl.ApplicationUserId == userId)
            .OrderByDescending(fl => fl.FillUpDate) // Order by FillUpDate (or another field)
            .AsQueryable();

        // Get the total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var fuelLogs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Return a PagedList object
        return new PagedList<FuelLog>(fuelLogs, pageNumber, pageSize,totalCount);
            
    }

    public Task<FuelLog?> GetFuelLogByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public void CreateFuelLog(FuelLog fuelLog)
    {
        context.FuelLogs.Add(fuelLog);
        context.SaveChanges();
    }

    public void UpdateFuelLog(FuelLog fuelLog)
    {
        throw new NotImplementedException();
    }

    public void DeleteFuelLog(FuelLog fuelLog)
    {
        throw new NotImplementedException();
    }
}