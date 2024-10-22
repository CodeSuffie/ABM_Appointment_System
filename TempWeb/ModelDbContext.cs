using Database.Models;
using Database.Models.EntityConfiguration;
using Microsoft.EntityFrameworkCore;

namespace TempWeb;

public class ModelDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    
    public DbSet<TruckCompany> TruckCompanies { get; set; }
    public DbSet<TruckYard> TruckYards { get; set; }
    
    public DbSet<Truck> Trucks { get; set; }
    public DbSet<Load> Loads { get; set; }
    public DbSet<Trip> Trips { get; set; }
    
    public DbSet<Hub> Hubs { get; set; }
    public DbSet<HubYard> HubYards { get; set; }
    
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<Bay> Bays { get; set; }
    
    public DbSet<TruckDriver> TruckDrivers { get; set; }
    public DbSet<AdminStaff> AdminStaffs { get; set; }
    public DbSet<BayStaff> BayStaffs { get; set; }
    
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<BayShift> BayShifts { get; set; }
    
    public DbSet<Work> Works { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
