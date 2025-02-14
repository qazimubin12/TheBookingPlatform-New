namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requestedemployee : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RequestedEmployees",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        GoogleCalendarID = c.String(),
                        WatchID = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RequestedEmployees");
        }
    }
}
