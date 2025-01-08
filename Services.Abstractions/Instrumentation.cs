using System.Diagnostics.Metrics;
using System.Text.Json;
using Database.Models;
using Microsoft.Extensions.Logging;

namespace Services.Abstractions;

public enum Metric
{
    Step,
    
    AdminWorking,
    BayStaffWorking,
    PickerWorking,
    StufferWorking,
    BayOpened,
    
    TruckOccupied,
    ParkingOccupied,
    BayOccupied,
    
    AdminOccupied,
    PickerOccupied,
    StufferOccupied,
    BayStaffDropOff,
    BayStaffPickUp,
    
    BayDroppedOff,
    BayFetched,
    BayPickedUp,
    
    PalletBay,
    
    DropOffMiss,
    FetchMiss,
    
    TripWaitTravel,
    TripTravelHub,
    TripWaitParking,
    TripWaitCheckIn,
    TripCheckingIn,
    TripWaitBay,
    TripBay,
    TripTravelHome,
    
    TripComplete
}

internal static class TupleExtensions
{
    public static KeyValuePair<T1, T2> ToKeyValuePair<T1, T2>(this (T1, T2) tuple)
    {
        return new KeyValuePair<T1, T2>(tuple.Item1, tuple.Item2);
    }
}

public class Instrumentation : IDisposable
{
    public const string ServiceName = "Simulator";  // Maybe Apps.ConsoleApp & Apps.DesktopApp?
    public const string ServiceVersion = "1.0.0";

    // private Dictionary<Metric, List<string>> _parameters = new Dictionary<Metric, List<string>>
    // {
    //     {
    //         Metric.Step, ["Truck", "Pallet"]
    //     }
    // };

    private Dictionary<Metric, Instrument> _instruments = [];

    private readonly ILogger<Instrumentation> _logger;
    private readonly ModelState _modelState;
    private readonly Meter _meter;
    
    public Instrumentation(
        ILogger<Instrumentation> logger,
        ModelState modelState)
    {
        _logger = logger;
        _modelState = modelState;
        _meter = new Meter(ServiceName, ServiceVersion);
        
        CreateCounter(Metric.Step, "Step", "Number of steps executed.");
        
        CreateUpDownCounter(Metric.AdminWorking, "AdminStaff", "#AdminStaff Working.");
        CreateUpDownCounter(Metric.BayStaffWorking, "BayStaff", "#BayStaff Working.");
        CreateUpDownCounter(Metric.PickerWorking, "Picker", "#Picker Working.");
        CreateUpDownCounter(Metric.StufferWorking, "Stuffer", "#Stuffer Working.");
        CreateUpDownCounter(Metric.BayOpened, "Bay", "#Bay Opened.");
        
        CreateUpDownCounter(Metric.TruckOccupied, "Truck", "#Truck Occupied.");
        CreateUpDownCounter(Metric.ParkingOccupied, "ParkingSpot", "#ParkingSpot Occupied.");
        CreateUpDownCounter(Metric.BayOccupied, "Bay", "#Bay Occupied.");
        
        CreateUpDownCounter(Metric.AdminOccupied, "AdminStaff", "#AdminStaff Occupied.");
        CreateUpDownCounter(Metric.PickerOccupied, "Picker", "#Picker Working on a Fetch.");
        CreateUpDownCounter(Metric.StufferOccupied, "Stuffer", "#Stuffer Working on a Stuff.");
        CreateUpDownCounter(Metric.BayStaffDropOff, "BayStaff", "#BayStaff Working on a Drop-Off.");
        CreateUpDownCounter(Metric.BayStaffPickUp, "BayStaff", "#BayStaff Working on a Pick-Up.");
        
        CreateUpDownCounter(Metric.BayDroppedOff, "Bay", "#Bays Finished Drop-Off.");
        CreateUpDownCounter(Metric.BayFetched, "Bay", "#Bays Finished Fetching.");
        CreateUpDownCounter(Metric.BayPickedUp, "Bay", "#Bays Working on a Pick-Up.");
        
        CreateUpDownCounter(Metric.PalletBay, "Pallet", "#Pallet at Bay.");
        
        CreateCounter(Metric.DropOffMiss, "DropOffMiss", "#Drop Off Pallets Unable to place at Bay.");
        CreateCounter(Metric.FetchMiss, "FetchMiss", "#PickUp Load not fetched yet.");
        
        CreateUpDownCounter(Metric.TripWaitTravel, "Trip", "#Trips Waiting to Travel to the Hub.");
        CreateUpDownCounter(Metric.TripTravelHub, "Trip", "#Trips Travelling to the Hub.");
        CreateUpDownCounter(Metric.TripWaitParking, "Trip", "#Trips Arrived at the Hub and waiting for a parking spot.");
        CreateUpDownCounter(Metric.TripWaitCheckIn, "Trip", "#Trips Parking but not yet Checked-In.");
        CreateUpDownCounter(Metric.TripCheckingIn, "Trip", "#Trips Currently Checking In.");
        CreateUpDownCounter(Metric.TripWaitBay, "Trip", "#Trips Checked In but not yet at a Bay.");
        CreateUpDownCounter(Metric.TripBay, "Trip", "#Trips Currently at a Bay.");
        CreateUpDownCounter(Metric.TripTravelHome, "Trip", "#Trips with completed Bay Work and Travelling home.");
        
        CreateCounter(Metric.TripComplete, "Trip", "#Trips Completed.");
        
        
    }
    
    private void CreateCounter(Metric metric, string? unit = null, string? description = null)
    {
        var metricName = JsonNamingPolicy.SnakeCaseLower.ConvertName(metric.ToString());
        var counter = _meter.CreateCounter<long>(metricName, unit, description);
        
        _instruments.Add(metric, counter);
    }

    private void CreateUpDownCounter(Metric metric, string? unit = null, string? description = null)
    {
        var metricName = JsonNamingPolicy.SnakeCaseLower.ConvertName(metric.ToString());
        var counter = _meter.CreateUpDownCounter<long>(metricName, unit, description);
        
        _instruments.Add(metric, counter);
    }

    public void Add(Metric metric, int count, params (string, object?)[] tags)
    {
        if (!_instruments.TryGetValue(metric, out var instrument))
        {
            _logger.LogError("Help. No value");
            return;
        }

        var stepTag = ("Step", (object?) _modelState.ModelTime).ToKeyValuePair();
        var tagArray = tags.Select(t => t.ToKeyValuePair()).Append(stepTag).ToArray();
        
        switch (instrument)
        {
            case UpDownCounter<long> upDownCounter:
                upDownCounter.Add(count, tagArray);
                break;
            case Counter<long> counter:
                counter.Add(count, tagArray);
                break;
            default:
                _logger.LogError("Help. No counter");
                break;
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
