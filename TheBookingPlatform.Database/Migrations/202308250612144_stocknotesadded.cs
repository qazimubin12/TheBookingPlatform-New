namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stocknotesadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockOrders", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockOrders", "Notes");
        }
    }
}
