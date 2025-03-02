namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class productid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "ProductID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "ProductID");
        }
    }
}
