namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bookinglinkaddded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "BookingLink", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "BookingLink");
        }
    }
}
