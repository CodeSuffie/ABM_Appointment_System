namespace Database.Models;

public class Load
{
    public long Id { get; set; }
    
    public LoadType LoadType { get; set; }

    public TruckCompany? TruckCompany { get; set; }
    public long TruckCompanyId { get; set; }

    public Hub? Hub { get; set; }
    public long HubId { get; set; }
    
    public Hub? Bay { get; set; }
    public long BayId { get; set; }
    
    public Trip? DropOffTrip { get; set; }
    public long? DropOffTripId { get; set; }
    
    public Trip? PickUpTrip { get; set; }
    public long? PickUpTripId { get; set; }
}
