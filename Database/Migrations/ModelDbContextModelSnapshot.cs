﻿// <auto-generated />
using System;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Database.Migrations
{
    [DbContext(typeof(ModelDbContext))]
    partial class ModelDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("Database.Models.AdminShift", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AdminStaffId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan?>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AdminStaffId");

                    b.ToTable("AdminShifts");
                });

            modelBuilder.Entity("Database.Models.AdminStaff", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("HubId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("WorkId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HubId");

                    b.HasIndex("WorkId")
                        .IsUnique();

                    b.ToTable("AdminStaffs");
                });

            modelBuilder.Entity("Database.Models.Bay", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("HubId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("XLocation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YLocation")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HubId");

                    b.ToTable("Bays");
                });

            modelBuilder.Entity("Database.Models.BayShift", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("BayId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("BayStaffId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan?>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BayId");

                    b.HasIndex("BayStaffId");

                    b.ToTable("BayShifts");
                });

            modelBuilder.Entity("Database.Models.BayStaff", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("HubId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("WorkId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HubId");

                    b.HasIndex("WorkId")
                        .IsUnique();

                    b.ToTable("BayStaffs");
                });

            modelBuilder.Entity("Database.Models.DropOffLoad", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("HubId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LocationId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("TruckCompanyId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("TruckId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HubId");

                    b.HasIndex("LocationId");

                    b.HasIndex("TruckCompanyId");

                    b.HasIndex("TruckId")
                        .IsUnique();

                    b.ToTable("DropOffLoads");
                });

            modelBuilder.Entity("Database.Models.Hub", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("XLocation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YLocation")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Hubs");
                });

            modelBuilder.Entity("Database.Models.Location", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("XLocation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YLocation")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Location");
                });

            modelBuilder.Entity("Database.Models.OperatingHour", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan?>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<long>("HubId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HubId");

                    b.ToTable("OperatingHours");
                });

            modelBuilder.Entity("Database.Models.ParkingSpot", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("HubId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("XLocation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YLocation")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HubId");

                    b.ToTable("ParkingSpots");
                });

            modelBuilder.Entity("Database.Models.PickUpLoad", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long?>("BayId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LocationId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TruckCompanyId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("TruckId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BayId");

                    b.HasIndex("LocationId");

                    b.HasIndex("TruckCompanyId");

                    b.HasIndex("TruckId")
                        .IsUnique();

                    b.ToTable("PickUpLoads");
                });

            modelBuilder.Entity("Database.Models.Trip", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("LocationId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("TruckId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("WorkId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.HasIndex("TruckId");

                    b.HasIndex("WorkId")
                        .IsUnique();

                    b.ToTable("Trips");
                });

            modelBuilder.Entity("Database.Models.Truck", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Capacity")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("DropOffLoadId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("PickUpLoadId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Planned")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TruckCompanyId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TruckCompanyId");

                    b.ToTable("Trucks");
                });

            modelBuilder.Entity("Database.Models.TruckCompany", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("XLocation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YLocation")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TruckCompanies");
                });

            modelBuilder.Entity("Database.Models.Work", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AdminStaffId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("BayStaffId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<long>("TripId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WorkType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Works");
                });

            modelBuilder.Entity("Database.Models.AdminShift", b =>
                {
                    b.HasOne("Database.Models.AdminStaff", "AdminStaff")
                        .WithMany("Shifts")
                        .HasForeignKey("AdminStaffId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AdminStaff");
                });

            modelBuilder.Entity("Database.Models.AdminStaff", b =>
                {
                    b.HasOne("Database.Models.Hub", "Hub")
                        .WithMany("AdminStaffs")
                        .HasForeignKey("HubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.Work", "Work")
                        .WithOne("AdminStaff")
                        .HasForeignKey("Database.Models.AdminStaff", "WorkId");

                    b.Navigation("Hub");

                    b.Navigation("Work");
                });

            modelBuilder.Entity("Database.Models.Bay", b =>
                {
                    b.HasOne("Database.Models.Hub", "Hub")
                        .WithMany("Bays")
                        .HasForeignKey("HubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Hub");
                });

            modelBuilder.Entity("Database.Models.BayShift", b =>
                {
                    b.HasOne("Database.Models.Bay", "Bay")
                        .WithMany("Shifts")
                        .HasForeignKey("BayId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.BayStaff", "BayStaff")
                        .WithMany("Shifts")
                        .HasForeignKey("BayStaffId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bay");

                    b.Navigation("BayStaff");
                });

            modelBuilder.Entity("Database.Models.BayStaff", b =>
                {
                    b.HasOne("Database.Models.Hub", "Hub")
                        .WithMany("BayStaffs")
                        .HasForeignKey("HubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.Work", "Work")
                        .WithOne("BayStaff")
                        .HasForeignKey("Database.Models.BayStaff", "WorkId");

                    b.Navigation("Hub");

                    b.Navigation("Work");
                });

            modelBuilder.Entity("Database.Models.DropOffLoad", b =>
                {
                    b.HasOne("Database.Models.Hub", "Hub")
                        .WithMany()
                        .HasForeignKey("HubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.TruckCompany", null)
                        .WithMany("UnloadLoads")
                        .HasForeignKey("TruckCompanyId");

                    b.HasOne("Database.Models.Truck", "Truck")
                        .WithOne("DropOffLoad")
                        .HasForeignKey("Database.Models.DropOffLoad", "TruckId");

                    b.Navigation("Hub");

                    b.Navigation("Location");

                    b.Navigation("Truck");
                });

            modelBuilder.Entity("Database.Models.OperatingHour", b =>
                {
                    b.HasOne("Database.Models.Hub", "Hub")
                        .WithMany("OperatingHours")
                        .HasForeignKey("HubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Hub");
                });

            modelBuilder.Entity("Database.Models.ParkingSpot", b =>
                {
                    b.HasOne("Database.Models.Hub", "Hub")
                        .WithMany("ParkingSpots")
                        .HasForeignKey("HubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Hub");
                });

            modelBuilder.Entity("Database.Models.PickUpLoad", b =>
                {
                    b.HasOne("Database.Models.Bay", null)
                        .WithMany("PickUpLoads")
                        .HasForeignKey("BayId");

                    b.HasOne("Database.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.TruckCompany", "TruckCompany")
                        .WithMany()
                        .HasForeignKey("TruckCompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.Truck", "Truck")
                        .WithOne("PickUpLoad")
                        .HasForeignKey("Database.Models.PickUpLoad", "TruckId");

                    b.Navigation("Location");

                    b.Navigation("Truck");

                    b.Navigation("TruckCompany");
                });

            modelBuilder.Entity("Database.Models.Trip", b =>
                {
                    b.HasOne("Database.Models.Location", "CurrentDestination")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Models.Truck", "Truck")
                        .WithMany()
                        .HasForeignKey("TruckId");

                    b.HasOne("Database.Models.Work", "Work")
                        .WithOne("Trip")
                        .HasForeignKey("Database.Models.Trip", "WorkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CurrentDestination");

                    b.Navigation("Truck");

                    b.Navigation("Work");
                });

            modelBuilder.Entity("Database.Models.Truck", b =>
                {
                    b.HasOne("Database.Models.TruckCompany", "TruckCompany")
                        .WithMany("Trucks")
                        .HasForeignKey("TruckCompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TruckCompany");
                });

            modelBuilder.Entity("Database.Models.AdminStaff", b =>
                {
                    b.Navigation("Shifts");
                });

            modelBuilder.Entity("Database.Models.Bay", b =>
                {
                    b.Navigation("PickUpLoads");

                    b.Navigation("Shifts");
                });

            modelBuilder.Entity("Database.Models.BayStaff", b =>
                {
                    b.Navigation("Shifts");
                });

            modelBuilder.Entity("Database.Models.Hub", b =>
                {
                    b.Navigation("AdminStaffs");

                    b.Navigation("BayStaffs");

                    b.Navigation("Bays");

                    b.Navigation("OperatingHours");

                    b.Navigation("ParkingSpots");
                });

            modelBuilder.Entity("Database.Models.Truck", b =>
                {
                    b.Navigation("DropOffLoad");

                    b.Navigation("PickUpLoad");
                });

            modelBuilder.Entity("Database.Models.TruckCompany", b =>
                {
                    b.Navigation("Trucks");

                    b.Navigation("UnloadLoads");
                });

            modelBuilder.Entity("Database.Models.Work", b =>
                {
                    b.Navigation("AdminStaff");

                    b.Navigation("BayStaff");

                    b.Navigation("Trip")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
