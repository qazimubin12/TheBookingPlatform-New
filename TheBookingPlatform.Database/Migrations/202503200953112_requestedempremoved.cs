namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requestedempremoved : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.RequestedEmployees");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RequestedEmployees",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        GoogleCalendarID = c.String(),
                        WatchID = c.String(),
                        WatchName = c.String(),
                        ExpirationDate = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
