namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pendingchangesplease : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoyaltyCardUsages",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CustomerID = c.Int(nullable: false),
                        CashBack = c.Single(nullable: false),
                        LoyaltyCardID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.Customers", "CashBack");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "CashBack", c => c.String());
            DropTable("dbo.LoyaltyCardUsages");
        }
    }
}
