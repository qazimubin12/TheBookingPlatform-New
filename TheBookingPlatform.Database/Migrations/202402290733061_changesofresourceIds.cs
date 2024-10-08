namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofresourceIds : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Appointments", "ResourceID");
            AddColumn("dbo.Appointments", "ResourceIDs", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "ResourceIDs");
            AddColumn("dbo.Appointments", "ResourceID", c => c.Int(nullable: false));
        }
    }
}
