namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ikdnew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "PriceChangeIDNotification", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "PriceChangeIDNotification");
        }
    }
}
