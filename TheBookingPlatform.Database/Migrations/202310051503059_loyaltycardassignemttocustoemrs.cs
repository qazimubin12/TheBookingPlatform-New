namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loyaltycardassignemttocustoemrs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoyaltyCardAssignments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CustomerID = c.Int(nullable: false),
                        LoyaltyCardID = c.Int(nullable: false),
                        CardNumber = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.Customers", "LoyaltyCards");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "LoyaltyCards", c => c.String());
            DropTable("dbo.LoyaltyCardAssignments");
        }
    }
}
