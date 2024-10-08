namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removedloayalycardusages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LoyaltyCardAssignments", "CashBack", c => c.Single(nullable: false));
            DropTable("dbo.LoyaltyCardUsages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LoyaltyCardUsages",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CustomerID = c.Int(nullable: false),
                        CashBack = c.Single(nullable: false),
                        LoyaltyCardID = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.LoyaltyCardAssignments", "CashBack");
        }
    }
}
