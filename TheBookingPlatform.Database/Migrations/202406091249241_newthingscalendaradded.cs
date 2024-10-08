namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newthingscalendaradded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GoogleCalendarIntegrations",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ClientID = c.String(),
                        ApiKEY = c.String(),
                        AccessToken = c.String(),
                        RefreshToken = c.String(),
                        Disabled = c.Boolean(nullable: false),
                        TypeOfIntegration = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Appointments", "GoogleCalendarEventID", c => c.String());
            AddColumn("dbo.Companies", "TimeZone", c => c.String());
            AddColumn("dbo.Employees", "GoogleCalendarName", c => c.String());
            AddColumn("dbo.Employees", "GoogleCalendarID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "GoogleCalendarID");
            DropColumn("dbo.Employees", "GoogleCalendarName");
            DropColumn("dbo.Companies", "TimeZone");
            DropColumn("dbo.Appointments", "GoogleCalendarEventID");
            DropTable("dbo.GoogleCalendarIntegrations");
        }
    }
}
