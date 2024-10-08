namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofdaysinexpiry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LoyaltyCards", "Days", c => c.Int(nullable: false));
            DropColumn("dbo.LoyaltyCards", "ExpirationDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LoyaltyCards", "ExpirationDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.LoyaltyCards", "Days");
        }
    }
}
