namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companycreatedby : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "CreatedBy", c => c.String());
            AddColumn("dbo.StockHistories", "ProductID", c => c.Int(nullable: false));
            AddColumn("dbo.StockOrders", "SupplierName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockOrders", "SupplierName");
            DropColumn("dbo.StockHistories", "ProductID");
            DropColumn("dbo.Companies", "CreatedBy");
        }
    }
}
