using Database.Models;
using Database.Models.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Database;

public sealed class ModelDbContext(DbContextOptions<ModelDbContext> options) : DbContext(options)
{
    public DbSet<TruckCompany> TruckCompanies { get; set; }
    public DbSet<Truck> Trucks { get; set; }
    
    public DbSet<DropOffLoad> DropOffLoads { get; set; }
    public DbSet<PickUpLoad> PickUpLoads { get; set; }
    public DbSet<Trip> Trips { get; set; }
    
    public DbSet<Hub> Hubs { get; set; }
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<Bay> Bays { get; set; }
    
    public DbSet<AdminStaff> AdminStaffs { get; set; }
    public DbSet<BayStaff> BayStaffs { get; set; }
    
    public DbSet<OperatingHour> OperatingHours { get; set; }
    
    public DbSet<AdminShift> AdminShifts { get; set; }
    public DbSet<BayShift> BayShifts { get; set; }
    
    public DbSet<Work> Works { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(WebConfig.DbConnectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TruckCompanyConfiguration());
        modelBuilder.ApplyConfiguration(new TruckConfiguration());
        
        modelBuilder.ApplyConfiguration(new DropOffLoadConfiguration());
        modelBuilder.ApplyConfiguration(new PickUpLoadConfiguration());
        modelBuilder.ApplyConfiguration(new TripConfiguration());
        
        modelBuilder.ApplyConfiguration(new HubConfiguration());
        modelBuilder.ApplyConfiguration(new ParkingSpotConfiguration());
        modelBuilder.ApplyConfiguration(new BayConfiguration());
        
        modelBuilder.ApplyConfiguration(new AdminStaffConfiguration());
        modelBuilder.ApplyConfiguration(new BayStaffConfiguration());
        
        modelBuilder.ApplyConfiguration(new OperatingHourConfiguration());
        
        modelBuilder.ApplyConfiguration(new AdminShiftConfiguration());
        modelBuilder.ApplyConfiguration(new BayShiftConfiguration());
        
        modelBuilder.ApplyConfiguration(new WorkConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
