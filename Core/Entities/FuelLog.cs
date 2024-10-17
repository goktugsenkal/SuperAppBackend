namespace Core.Entities;

public class FuelLog : EntityBase
{
    public DateTime FillUpDate { get; set; }
    public decimal FuelVolumeFilled { get; set; }
    public decimal FuelVolumeCalculated { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalCost { get; set; }
    public int OdometerReading { get; set; }
    public string? FuelStation { get; set; }
    public string? FuelType { get; set; }
    public string? PaymentMethod { get; set; } 
    public bool IsTankFull { get; set; }
    public string? VehiclePlateNumber { get; set; }
    public decimal? FuelEfficiency { get; set; }
    public string? Notes { get; set; }

    public string ApplicationUserId { get; set; }
}