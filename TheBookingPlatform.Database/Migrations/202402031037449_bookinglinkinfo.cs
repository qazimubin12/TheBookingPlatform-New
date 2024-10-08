namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bookinglinkinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "BookingLinkInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "BookingLinkInfo");
        }
    }
}
