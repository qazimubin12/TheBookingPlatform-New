namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductsModule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        SalesPrice = c.Single(nullable: false),
                        CostPrice = c.Single(nullable: false),
                        EANCode = c.String(),
                        VAT = c.String(),
                        ManageStockOrder = c.Boolean(nullable: false),
                        MinStock = c.Int(nullable: false),
                        MaxStock = c.Int(nullable: false),
                        CurrentStock = c.Int(nullable: false),
                        CategoryID = c.Int(nullable: false),
                        SupplierID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.StockHistories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Amount = c.Int(nullable: false),
                        Reason = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.StockOrders",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SupplierID = c.Int(nullable: false),
                        ProductDetails = c.String(),
                        GrandTotal = c.Single(nullable: false),
                        Status = c.String(),
                        IsDraft = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        OrderedDate = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Suppliers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Address = c.String(),
                        PostalCode = c.Int(nullable: false),
                        City = c.String(),
                        TotalInventory = c.Single(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Suppliers");
            DropTable("dbo.StockOrders");
            DropTable("dbo.StockHistories");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
