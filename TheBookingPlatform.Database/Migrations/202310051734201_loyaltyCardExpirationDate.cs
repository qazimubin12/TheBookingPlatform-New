namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loyaltyCardExpirationDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LoyaltyCards", "ExpirationDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.LoyaltyCards", "CardNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LoyaltyCards", "CardNumber", c => c.String());
            DropColumn("dbo.LoyaltyCards", "ExpirationDate");
        }
    }
}
