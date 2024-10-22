namespace Database.Models;

public class TruckDriver : Staff
{
    // Staff
    public new List<TruckShift> Shifts { get; set; } = [];
    
    // TruckDriver
    public required TruckCompany TruckCompany { get; set; }
    public long TruckCompanyId { get; set; }
}
