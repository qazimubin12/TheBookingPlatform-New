namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class saleproductstypeandsaleschanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sales", "Type", c => c.String());
            DropColumn("dbo.SaleProducts", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SaleProducts", "Type", c => c.String());
            DropColumn("dbo.Sales", "Type");
        }
    }
}
