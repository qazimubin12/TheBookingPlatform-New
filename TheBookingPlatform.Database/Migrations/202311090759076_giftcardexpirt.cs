namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardexpirt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftCardAssignments", "AssignedCode", c => c.String());
            AddColumn("dbo.GiftCards", "Days", c => c.Int(nullable: false));
            DropColumn("dbo.GiftCards", "GiftCardExpiry");
            DropColumn("dbo.GiftCards", "HaveExpiry");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GiftCards", "HaveExpiry", c => c.Boolean(nullable: false));
            AddColumn("dbo.GiftCards", "GiftCardExpiry", c => c.String());
            DropColumn("dbo.GiftCards", "Days");
            DropColumn("dbo.GiftCardAssignments", "AssignedCode");
        }
    }
}
