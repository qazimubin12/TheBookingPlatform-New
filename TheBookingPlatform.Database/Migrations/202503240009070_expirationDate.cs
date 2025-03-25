namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class expirationDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoogleCalendarIntegrations", "ExpirationDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GoogleCalendarIntegrations", "ExpirationDate");
        }
    }
}
