namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class saleproducts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SaleProducts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        ProductID = c.Int(nullable: false),
                        Qty = c.Single(nullable: false),
                        Total = c.Single(nullable: false),
                        Date = c.DateTime(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SaleProducts");
        }
    }
}
