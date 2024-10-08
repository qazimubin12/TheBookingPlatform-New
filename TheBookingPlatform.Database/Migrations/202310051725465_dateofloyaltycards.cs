namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dateofloyaltycards : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LoyaltyCardAssignments", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.LoyaltyCardPromotions", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.LoyaltyCards", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.LoyaltyCardUsages", "Date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LoyaltyCardUsages", "Date");
            DropColumn("dbo.LoyaltyCards", "Date");
            DropColumn("dbo.LoyaltyCardPromotions", "Date");
            DropColumn("dbo.LoyaltyCardAssignments", "Date");
        }
    }
}
