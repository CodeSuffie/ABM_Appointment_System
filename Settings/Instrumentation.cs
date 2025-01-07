using System.Diagnostics.Metrics;

namespace Settings;

public class Instrumentation : IDisposable
{
    public const string ServiceName = "Simulator";  // Maybe Apps.ConsoleApp & Apps.DesktopApp?
    public const string ServiceVersion = "1.0.0";

    private readonly Meter _meter;
    
    public Counter<long> StepCounter { get; init; }
    
    public UpDownCounter<long> OccupiedTruckCounter { get; init; }
    public UpDownCounter<long> OccupiedAdminStaffCounter { get; init; }
    public UpDownCounter<long> OccupiedParkingSpotCounter { get; init; }
    public UpDownCounter<long> OccupiedBayCounter { get; init; }
    
    public UpDownCounter<long> WaitTravelHubTripsCounter { get; init; }
    public UpDownCounter<long> TravelHubTripsCounter { get; init; }
    public UpDownCounter<long> WaitParkingTripsCounter { get; init; }
    public UpDownCounter<long> ParkedTripsCounter { get; init; }
    public UpDownCounter<long> CheckingInTripsCounter { get; init; }
    public UpDownCounter<long> WaitBayTripsCounter { get; init; }
    public UpDownCounter<long> AtBayTripsCounter { get; init; }
    public UpDownCounter<long> TravelHomeTripsCounter { get; init; }
    
    public Counter<long> CompletedTripsCounter { get; init; }
    
    public UpDownCounter<long> DroppedOffBaysCounter { get; init; }
    public UpDownCounter<long> FetchedBaysCounter { get; init; }
    public UpDownCounter<long> PickedUpBaysCounter { get; init; }
    
    public Counter<long> DropOffMissCounter { get; init; }
    public UpDownCounter<long> DropOffBayStaffCounter { get; init; }
    public UpDownCounter<long> PickUpBayStaffCounter { get; init; }
    
    public Counter<long> FetchMissCounter { get; init; }
    public UpDownCounter<long> OccupiedPickerCounter { get; init; }
        
    public UpDownCounter<long> OccupiedStufferCounter { get; init; }
    public Counter<long> WorkingAdminStaffCounter { get; init; }
    public Counter<long> WorkingBayStaffCounter { get; init; }
    public Counter<long> WorkingPickerCounter { get; init; }
    public Counter<long> WorkingStufferCounter { get; init; }
    
    public Instrumentation()
    {
        _meter = new Meter(ServiceName, ServiceVersion);
        
        StepCounter = _meter.CreateCounter<long>("steps", "Steps", "Number of steps executed.");
        
        OccupiedTruckCounter = _meter.CreateUpDownCounter<long>("occupied-trucks", "Truck", "#Truck Occupied.");
        OccupiedAdminStaffCounter = _meter.CreateUpDownCounter<long>("occupied-admin-staff", "AdminStaff", "#AdminStaff Occupied.");
        OccupiedParkingSpotCounter = _meter.CreateUpDownCounter<long>("occupied-parking-spots", "ParkingSpot", "#ParkingSpot Occupied.");
        OccupiedBayCounter = _meter.CreateUpDownCounter<long>("occupied-bays", "Bay", "#Bay Occupied.");
        
        WaitTravelHubTripsCounter = _meter.CreateUpDownCounter<long>("wait-travel-hub-trip", "Trip", "#Trips Waiting to Travel to the Hub.");
        TravelHubTripsCounter = _meter.CreateUpDownCounter<long>("travel-hub-trip", "Trip", "#Trips Travelling to the Hub.");
        WaitParkingTripsCounter = _meter.CreateUpDownCounter<long>("arrived-hub-trip", "Trip", "#Trips Arrived at the Hub but not yet parking.");
        ParkedTripsCounter = _meter.CreateUpDownCounter<long>("parking-trip", "Trip", "#Trips Parking but not yet Checked-In.");
        CheckingInTripsCounter = _meter.CreateUpDownCounter<long>("checking-in-trip", "Trip", "#Trips Currently Checking In.");
        WaitBayTripsCounter = _meter.CreateUpDownCounter<long>("checked-in-trip", "Trip", "#Trips Checked In but not yet at a Bay.");
        AtBayTripsCounter = _meter.CreateUpDownCounter<long>("bay-trip", "Trip", "#Trips Currently at a Bay.");
        TravelHomeTripsCounter = _meter.CreateUpDownCounter<long>("travel-home-trip", "Trip", "#Trips with completed Bay Work and Travelling home.");
        
        CompletedTripsCounter = _meter.CreateCounter<long>("completed-trip", "Trip", "#Trips Completed.");
        
        DroppedOffBaysCounter = _meter.CreateUpDownCounter<long>("dropped-off-bay", "Bay", "#Bays Finished Drop-Off.");
        FetchedBaysCounter = _meter.CreateUpDownCounter<long>("fetched-bay", "Bay", "#Bays Finished Fetching.");
        PickedUpBaysCounter = _meter.CreateUpDownCounter<long>("picking-up-bay", "Bay", "#Bays Working on a Pick-Up.");
        
        DropOffMissCounter = _meter.CreateCounter<long>("drop-off-miss", "DropOffMiss", "#Drop Off Pellets Unable to place at Bay.");
        DropOffBayStaffCounter = _meter.CreateUpDownCounter<long>("drop-off-bay-staff", "BayStaff", "#BayStaff Working on a Drop-Off.");
        PickUpBayStaffCounter = _meter.CreateUpDownCounter<long>("pick-up-bay-staff", "BayStaff", "#BayStaff Working on a Pick-Up.");

        FetchMissCounter = _meter.CreateCounter<long>("fetch-miss", "FetchMiss", "#PickUp Load not fetched yet.");
        OccupiedPickerCounter = _meter.CreateUpDownCounter<long>("fetching-picker", "Picker", "#Picker Working on a Fetch.");
        
        OccupiedStufferCounter = _meter.CreateUpDownCounter<long>("fetch-stuffer", "Stuffer", "#Stuffer Working on a Stuff.");
        
        WorkingAdminStaffCounter = _meter.CreateCounter<long>("working-admin-staff", "AdminStaff", "#AdminStaff Working.");
        WorkingBayStaffCounter = _meter.CreateCounter<long>("working-bay-staff", "BayStaff", "#BayStaff Working.");
        WorkingPickerCounter = _meter.CreateCounter<long>("working-picker", "Picker", "#Picker Working.");
        WorkingStufferCounter = _meter.CreateCounter<long>("working-stuffer", "Stuffer", "#Stuffer Working.");
    }

    public void Dispose()
    {
        _meter.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
