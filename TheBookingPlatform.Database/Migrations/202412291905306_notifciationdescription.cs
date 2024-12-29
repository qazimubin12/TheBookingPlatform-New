namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notifciationdescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Description");
        }
    }
}
