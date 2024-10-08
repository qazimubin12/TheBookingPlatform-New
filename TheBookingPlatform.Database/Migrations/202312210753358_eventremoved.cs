namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eventremoved : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Events");
        }
        
        public override void Down()
        {
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
                        BookingDate = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
