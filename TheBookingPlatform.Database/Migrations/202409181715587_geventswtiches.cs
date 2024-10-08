namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class geventswtiches : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.GEventSwitches", "Busienss");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GEventSwitches", "Busienss", c => c.String());
        }
    }
}
