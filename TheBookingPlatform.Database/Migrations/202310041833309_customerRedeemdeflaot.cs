namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customerRedeemdeflaot : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "CashBack", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "CashBack", c => c.Single(nullable: false));
        }
    }
}
