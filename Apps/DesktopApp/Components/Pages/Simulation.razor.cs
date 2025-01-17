﻿using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DesktopApp.Components.Pages;

public sealed partial class Simulation
{
    private const string JavaScriptFilePath = "./Components/Pages/Simulation.razor.js";
    private IJSObjectReference? _javaScriptModule;
    private DotNetObjectReference<Simulation>? _javaScriptObjRef;
    
    private Timer? _timer;
    private SemaphoreSlim _semaphore = new(1, 1);
    private List<Truck> _currentTrucks = [];
    private List<Bay> _currentBays = [];

    private bool _isRunning = false;
    private bool _sidebarVisible = false;
    private Truck? _selectedTruck = null;
    private Bay? _selectedBay = null;
    
    private bool Disposed { get; set; }

    protected override void OnInitialized()
    {
        _javaScriptObjRef = DotNetObjectReference.Create(this);
        
        base.OnInitialized();
    }

    private string GetPlayPauseButtonClass()
    {
        return _isRunning
            ? "btn btn-warning"
            : "btn btn-success";
    }

    private string GetPlayPauseButtonIconClass()
    {
        return _isRunning
            ? "bi bi-pause-fill"
            : "bi bi-play-fill";
    }

    private void OnPlayPauseButtonClicked()
    { 
        _isRunning = !_isRunning;
        StateHasChanged();
    }

    [JSInvokable]
    public async Task ShowTruckInformationAsync(long truckId)
    {
        // attempt to find truck by truck id
        var truck = _currentTrucks.FirstOrDefault(x => x.Id == truckId);
        if (truck == null)
        {
            return;
        }
        
        // show truck information
        _sidebarVisible = true;
        _selectedBay = null;
        _selectedTruck = truck;
        
        // update state
        await InvokeAsync(StateHasChanged);
        await ResizeRendererAsync();
    }
    
    [JSInvokable]
    public async Task ShowBayInformationAsync(long bayId)
    {
        // attempt to find bay by bay id
        var bay = _currentBays.FirstOrDefault(x => x.Id == bayId);
        if (bay == null)
        {
            return;
        }
        
        // show bay information
        _sidebarVisible = true;
        _selectedTruck = null;
        _selectedBay = bay;
        
        // update state
        await InvokeAsync(StateHasChanged);
        await ResizeRendererAsync();
    }

    public async Task CloseSidebarAsync()
    {
        // hide sidebar
        _sidebarVisible = false;
        _selectedTruck = null;
        _selectedBay = null;
        
        // update state
        await InvokeAsync(StateHasChanged);
        await ResizeRendererAsync();
    }
    
    private async ValueTask LoadJavaScriptModuleAsync()
    {
        if (_javaScriptModule != null)
        {
            return;
        }
        
        _javaScriptModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JavaScriptFilePath);
        await _javaScriptModule.InvokeVoidAsync("initialize", _javaScriptObjRef);
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

            var hubBayMinX = hubBays.Min(b => b.XLocation);
            var hubBayMaxX = hubBays.Max(b => b.XLocation);
            var hubBayMinY = hubBays.Min(b => b.YLocation);
            var hubBayMaxY = hubBays.Max(b => b.YLocation);
            
            var minX = Math.Min(hubParkingSpots.Min(p => p.XLocation), hubBayMinX);
            var maxX = Math.Max(hubParkingSpots.Max(p => p.XLocation), hubBayMaxX);
            var minY = Math.Min(hubParkingSpots.Min(p => p.YLocation), hubBayMinY);
            var maxY = Math.Max(hubParkingSpots.Max(p => p.YLocation), hubBayMaxY);

            await AddBoundariesAsync(hub.Id, minX - 1, maxX + 1, minY - 1, maxY + 1);
            await AddHubAsync(hub.Id, hubBayMinX, hubBayMaxX + 1, hubBayMinY, hubBayMaxY + 1);
        }
        
        // create new timer
        _isRunning = true;
        _timer = new Timer(TimerCallback, null, 0, 50);
        StateHasChanged();
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

    private async ValueTask ResizeRendererAsync()
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("resizeRenderer");
    }
    
    private async ValueTask AddBayAsync(Bay bay)
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("addBay", 
            bay.Id, bay.XLocation, bay.YLocation, bay.XSize, bay.YSize);
    }
    
    private async ValueTask AddBaysAsync(Bay[] bays)
    {
        foreach (var bay in bays)
        {
            await AddBayAsync(bay);
        }
        
        // add new bay to list
        _currentBays.AddRange(bays);
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
    
    private async ValueTask AddHubAsync(long id, long minX, long maxX, long minY, long maxY)
    {
        if (_javaScriptModule == null)
        {
            return;
        }
        
        await _javaScriptModule.InvokeVoidAsync("addHub", 
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
            parkingSpot.Id, parkingSpot.XLocation, parkingSpot.YLocation,
            1,
            1);
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
            truck.Id, truck.Trip.XLocation, truck.Trip.YLocation);
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
        // exit early if we should not do anything
        if (!_isRunning)
        {
            return;
        }
        
        // lock semaphore
        await _semaphore.WaitAsync();
        
        // exit early if we should not do anything
        if (!_isRunning)
        {
            _semaphore.Release();
            return;
        }
        
        // run model frame
        await ModelService.RunFrameAsync();
        
        // update 3d state
        var trucks = await ModelDbContext.Trucks.Where(x => x.Trip != null).ToArrayAsync();
        _currentBays = await ModelDbContext.Bays.ToListAsync();
        
        // remove trucks that are no longer visible
        await RemoveTrucksAsync(trucks);
        
        // add/update trucks
        await AddTrucksAsync(trucks);
        
        // unlock semaphore
        _semaphore.Release();
        
        // update state
        await InvokeAsync(StateHasChanged);
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

        _isRunning = false;
        if (_timer != null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }
        
        // dispose ThreeJS renderer
        await DisposeRendererAsync();
        
        // dispose JS object reference
        _javaScriptObjRef?.Dispose();
        
        Disposed = true;
    }

    private string GetValueNowString()
    {
        var part = ModelState.ModelTime / ModelState.ModelConfig.ModelTotalTime;
        var roundedPercentage = (int)(part * 100);
        return roundedPercentage.ToString();
    }
}
