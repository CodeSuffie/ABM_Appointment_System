using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DesktopApp.Components.Pages;

public sealed partial class Simulation
{
    private const string JavaScriptFilePath = "./Components/Pages/Simulation.razor.js";
    private IJSObjectReference? _javaScriptModule;
    private Timer? _timer;
    
    private SemaphoreSlim _semaphore = new(1, 1);

    private List<Truck> _currentTrucks = [];
    
    private bool Disposed { get; set; }
    
    private async ValueTask LoadJavaScriptModuleAsync()
    {
        if (_javaScriptModule != null)
        {
            return;
        }
        
        _javaScriptModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JavaScriptFilePath);
        await _javaScriptModule.InvokeVoidAsync("initialize");
    }

    private async ValueTask InitializeModelServiceAsync()
    {
        // destroy timer if it already exists
        if (_timer != null)
        {
            await _timer.DisposeAsync();
        }
        
        // initialize model service
        await ModelService.InitializeAsync();

        // add parking spots
        var hubs = ModelDbContext.Hubs.ToArray();
        var parkingSpots = ModelDbContext.ParkingSpots.ToArray();
        var bays = ModelDbContext.Bays.ToArray();
        await AddParkingSpotsAsync(parkingSpots);
        await AddBaysAsync(bays);

        // loop over hubs, calculate boundaries
        foreach (var hub in hubs)
        {
            var hubParkingSpots = parkingSpots.Where(p => p.HubId == hub.Id).ToArray();
            var hubBays = bays.Where(b => b.HubId == hub.Id).ToArray();

            var minX = Math.Min(hubParkingSpots.Min(p => p.XLocation), hubBays.Min(b => b.XLocation));
            var maxX = Math.Max(hubParkingSpots.Max(p => p.XLocation), hubBays.Max(b => b.XLocation));
            var minY = Math.Min(hubParkingSpots.Min(p => p.YLocation), hubBays.Min(b => b.YLocation));
            var maxY = Math.Max(hubParkingSpots.Max(p => p.YLocation), hubBays.Max(b => b.YLocation));

            await AddBoundariesAsync(hub.Id, minX - 1, maxX + 1, minY - 1, maxY + 1);
        }
        
        // create new timer
        _timer = new Timer(TimerCallback, null, 0, 100);
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadJavaScriptModuleAsync();
            await InitializeModelServiceAsync();
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async ValueTask AddBayAsync(Bay bay)
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("addBay", 
            bay.Id,
            bay.XLocation,
            bay.YLocation,
            bay.XSize,
            bay.YSize);
    }
    
    private async ValueTask AddBaysAsync(Bay[] bays)
    {
        foreach (var bay in bays)
        {
            await AddBayAsync(bay);
        }
    }
    
    private async ValueTask AddBoundariesAsync(long id, long minX, long maxX, long minY, long maxY)
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("addBoundaries", 
            id,
            minX,
            maxX,
            minY,
            maxY);
    }
    
    private async ValueTask AddParkingSpotAsync(ParkingSpot parkingSpot)
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("addParkingSpot", 
            parkingSpot.Id,
            parkingSpot.XLocation,
            parkingSpot.YLocation,
            parkingSpot.XSize,
            parkingSpot.YSize);
    }

    private async ValueTask AddParkingSpotsAsync(ParkingSpot[] parkingSpots)
    {
        foreach (var parkingSpot in parkingSpots)
        {
            await AddParkingSpotAsync(parkingSpot);
        }
    }
    
    private async ValueTask AddTruckAsync(Truck truck)
    {
        if (_javaScriptModule == null || truck.Trip == null)
        {
            return;
        }

        await _javaScriptModule.InvokeVoidAsync("addTruck", 
            truck.Id,
            truck.Trip.XLocation,
            truck.Trip.YLocation);
    }
    
    private async ValueTask AddTrucksAsync(Truck[] trucks)
    {
        foreach (var truck in trucks)
        {
            await AddTruckAsync(truck);
        }
        
        // add new trucks to list
        _currentTrucks.AddRange(trucks);
    }

    private async ValueTask RemoveTruckAsync(Truck truck)
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("removeTruck", truck.Id);
    }
    
    private async ValueTask RemoveTrucksAsync(Truck[] visibleTrucks)
    {
        var removedTrucks = _currentTrucks.Where(t => visibleTrucks.All(v => v.Id != t.Id)).ToArray();

        foreach (var truck in removedTrucks)
        {
            await RemoveTruckAsync(truck);
        }
        
        // remove trucks from list
        _currentTrucks.Clear();
    }
    
    private async void TimerCallback(object? state)
    {
        // lock semaphore
        await _semaphore.WaitAsync();
        
        // run model frame
        await ModelService.RunFrameAsync();
        
        // update 3d state
        var trucks = await ModelDbContext.Trucks.Where(x => x.Trip != null).ToArrayAsync();
        
        // remove trucks that are no longer visible
        await RemoveTrucksAsync(trucks);
        
        // add/update trucks
        await AddTrucksAsync(trucks);
        
        // unlock semaphore
        _semaphore.Release();
    }
    
    private async ValueTask DisposeRendererAsync()
    {
        if (_javaScriptModule == null)
        {
            return;
        }

        await _javaScriptModule.InvokeVoidAsync("dispose");
        _javaScriptModule = null;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;

        if (_timer != null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }
        
        // dispose ThreeJS renderer
        await DisposeRendererAsync();
    }
}
