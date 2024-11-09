using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class version1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hubs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    XLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    YLocation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    XLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    YLocation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TruckCompanies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    XLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    YLocation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Works",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WorkType = table.Column<int>(type: "INTEGER", nullable: false),
                    TripId = table.Column<long>(type: "INTEGER", nullable: false),
                    AdminStaffId = table.Column<long>(type: "INTEGER", nullable: false),
                    BayStaffId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Works", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    XLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    YLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    HubId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bays_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperatingHours",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HubId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatingHours_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    XLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    YLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    HubId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSpots_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Planned = table.Column<bool>(type: "INTEGER", nullable: false),
                    TruckCompanyId = table.Column<long>(type: "INTEGER", nullable: false),
                    DropOffLoadId = table.Column<long>(type: "INTEGER", nullable: true),
                    PickUpLoadId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trucks_TruckCompanies_TruckCompanyId",
                        column: x => x.TruckCompanyId,
                        principalTable: "TruckCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminStaffs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HubId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminStaffs_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminStaffs_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BayStaffs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HubId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayStaffs_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BayStaffs_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DropOffLoads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<long>(type: "INTEGER", nullable: false),
                    HubId = table.Column<long>(type: "INTEGER", nullable: false),
                    TruckId = table.Column<long>(type: "INTEGER", nullable: true),
                    TruckCompanyId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropOffLoads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DropOffLoads_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DropOffLoads_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DropOffLoads_TruckCompanies_TruckCompanyId",
                        column: x => x.TruckCompanyId,
                        principalTable: "TruckCompanies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DropOffLoads_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PickUpLoads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<long>(type: "INTEGER", nullable: false),
                    TruckCompanyId = table.Column<long>(type: "INTEGER", nullable: false),
                    TruckId = table.Column<long>(type: "INTEGER", nullable: true),
                    BayId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickUpLoads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickUpLoads_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PickUpLoads_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PickUpLoads_TruckCompanies_TruckCompanyId",
                        column: x => x.TruckCompanyId,
                        principalTable: "TruckCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PickUpLoads_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<long>(type: "INTEGER", nullable: false),
                    TruckId = table.Column<long>(type: "INTEGER", nullable: true),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trips_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trips_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminShifts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminStaffId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminShifts_AdminStaffs_AdminStaffId",
                        column: x => x.AdminStaffId,
                        principalTable: "AdminStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BayShifts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BayId = table.Column<long>(type: "INTEGER", nullable: false),
                    BayStaffId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayShifts_BayStaffs_BayStaffId",
                        column: x => x.BayStaffId,
                        principalTable: "BayStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BayShifts_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminShifts_AdminStaffId",
                table: "AdminShifts",
                column: "AdminStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminStaffs_HubId",
                table: "AdminStaffs",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminStaffs_WorkId",
                table: "AdminStaffs",
                column: "WorkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bays_HubId",
                table: "Bays",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_BayShifts_BayId",
                table: "BayShifts",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_BayShifts_BayStaffId",
                table: "BayShifts",
                column: "BayStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_BayStaffs_HubId",
                table: "BayStaffs",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_BayStaffs_WorkId",
                table: "BayStaffs",
                column: "WorkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DropOffLoads_HubId",
                table: "DropOffLoads",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_DropOffLoads_LocationId",
                table: "DropOffLoads",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DropOffLoads_TruckCompanyId",
                table: "DropOffLoads",
                column: "TruckCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DropOffLoads_TruckId",
                table: "DropOffLoads",
                column: "TruckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperatingHours_HubId",
                table: "OperatingHours",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_HubId",
                table: "ParkingSpots",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_PickUpLoads_BayId",
                table: "PickUpLoads",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_PickUpLoads_LocationId",
                table: "PickUpLoads",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PickUpLoads_TruckCompanyId",
                table: "PickUpLoads",
                column: "TruckCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PickUpLoads_TruckId",
                table: "PickUpLoads",
                column: "TruckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_LocationId",
                table: "Trips",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TruckId",
                table: "Trips",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_WorkId",
                table: "Trips",
                column: "WorkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_TruckCompanyId",
                table: "Trucks",
                column: "TruckCompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminShifts");

            migrationBuilder.DropTable(
                name: "BayShifts");

            migrationBuilder.DropTable(
                name: "DropOffLoads");

            migrationBuilder.DropTable(
                name: "OperatingHours");

            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "PickUpLoads");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "AdminStaffs");

            migrationBuilder.DropTable(
                name: "BayStaffs");

            migrationBuilder.DropTable(
                name: "Bays");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Trucks");

            migrationBuilder.DropTable(
                name: "Works");

            migrationBuilder.DropTable(
                name: "Hubs");

            migrationBuilder.DropTable(
                name: "TruckCompanies");
        }
    }
}
