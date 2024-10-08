namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LoyaltycardandPromotions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoyaltyCardPromotions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LoyaltyCardID = c.Int(nullable: false),
                        Percentage = c.Single(nullable: false),
                        Services = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.LoyaltyCards",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CardNumber = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Customers", "CashBack", c => c.Single(nullable: false));
            AddColumn("dbo.Customers", "LoyaltyCards", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "LoyaltyCards");
            DropColumn("dbo.Customers", "CashBack");
            DropTable("dbo.LoyaltyCards");
            DropTable("dbo.LoyaltyCardPromotions");
        }
    }
}
