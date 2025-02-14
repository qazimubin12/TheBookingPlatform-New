namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requestedemployee2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestedEmployees", "WatchName", c => c.String());
            AddColumn("dbo.RequestedEmployees", "ExpirationDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestedEmployees", "ExpirationDate");
            DropColumn("dbo.RequestedEmployees", "WatchName");
        }
    }
}
