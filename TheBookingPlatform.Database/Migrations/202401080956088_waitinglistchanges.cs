namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class waitinglistchanges : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WaitingLists",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        Service = c.String(),
                        Color = c.String(),
                        ServiceDuration = c.String(),
                        ServiceDiscount = c.String(),
                        Date = c.DateTime(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Notes = c.String(),
                        TotalCost = c.Single(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        BookingDate = c.DateTime(nullable: false),
                        Reminder = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.Appointments", "IsInWaiting");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Appointments", "IsInWaiting", c => c.Boolean(nullable: false));
            DropTable("dbo.WaitingLists");
        }
    }
}
