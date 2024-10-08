namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofaddingservicediscount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "ServiceDiscount", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "ServiceDiscount");
        }
    }
}
