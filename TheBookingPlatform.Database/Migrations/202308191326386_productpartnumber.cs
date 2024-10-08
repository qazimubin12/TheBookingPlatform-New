namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class productpartnumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PartNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "PartNumber");
        }
    }
}
