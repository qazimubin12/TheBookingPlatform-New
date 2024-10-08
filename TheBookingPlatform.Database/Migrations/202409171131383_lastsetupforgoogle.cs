namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class lastsetupforgoogle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoogleCalendarIntegrations", "InitialSetup", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GoogleCalendarIntegrations", "InitialSetup");
        }
    }
}
