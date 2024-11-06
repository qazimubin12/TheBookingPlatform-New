namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class saleproductstype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SaleProducts", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SaleProducts", "Type");
        }
    }
}
