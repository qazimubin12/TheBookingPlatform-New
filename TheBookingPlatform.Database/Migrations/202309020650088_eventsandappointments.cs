namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eventsandappointments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Appointments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Service = c.String(),
                        Date = c.DateTime(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Frequency = c.String(),
                        Every = c.String(),
                        Days = c.String(),
                        Ends = c.String(),
                        Notes = c.String(),
                        TotalCost = c.Single(nullable: false),
                        IsWalkIn = c.Boolean(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        BookingDate = c.DateTime(nullable: false),
                        Label = c.String(),
                        NoShow = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EventType = c.String(),
                        Description = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        StartTime = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        IsAllDay = c.Boolean(nullable: false),
                        AppliesTo = c.String(),
                        EmployeeID = c.Int(nullable: false),
                        Frequency = c.String(),
                        Every = c.String(),
                        Days = c.String(),
                        Ends = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Events");
            DropTable("dbo.Appointments");
        }
    }
}
