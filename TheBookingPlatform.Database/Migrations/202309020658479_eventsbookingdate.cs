namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eventsbookingdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "BookingDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "BookingDate");
        }
    }
}
