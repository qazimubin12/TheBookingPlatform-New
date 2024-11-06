namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class saleproductstypeandsales : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sales",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        Remarks = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.SaleProducts", "ReferenceID", c => c.Int(nullable: false));
            AddColumn("dbo.SaleProducts", "Type", c => c.String());
            DropColumn("dbo.SaleProducts", "AppointmentID");
            DropColumn("dbo.SaleProducts", "Date");
            DropColumn("dbo.SaleProducts", "CustomerID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SaleProducts", "CustomerID", c => c.Int(nullable: false));
            AddColumn("dbo.SaleProducts", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.SaleProducts", "AppointmentID", c => c.Int(nullable: false));
            DropColumn("dbo.SaleProducts", "Type");
            DropColumn("dbo.SaleProducts", "ReferenceID");
            DropTable("dbo.Sales");
        }
    }
}
