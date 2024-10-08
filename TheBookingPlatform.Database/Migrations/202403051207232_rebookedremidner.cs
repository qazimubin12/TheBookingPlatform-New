namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebookedremidner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "BookedFromRebook", c => c.Boolean(nullable: false));
            DropColumn("dbo.Appointments", "ResourceIDs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Appointments", "ResourceIDs", c => c.String());
            DropColumn("dbo.Appointments", "BookedFromRebook");
        }
    }
}
