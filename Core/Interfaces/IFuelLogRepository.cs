using Core.Models;

namespace Core.Interfaces;

public interface IFuelLogRepository
{
    Task<PagedList<FuelLog>> GetAllFuelLogsAsync(string userId, int pageNumber, int pageSize);
    Task<FuelLog?> GetFuelLogByIdAsync(int id);
    void CreateFuelLog(FuelLog fuelLog);
    void UpdateFuelLog(FuelLog fuelLog);
    void DeleteFuelLog(FuelLog fuelLog);
}