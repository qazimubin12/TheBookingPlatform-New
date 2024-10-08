namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardDaysRemovedgiftcardassignmentdaysadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftCardAssignments", "Days", c => c.Int(nullable: false));
            DropColumn("dbo.GiftCards", "Days");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GiftCards", "Days", c => c.Int(nullable: false));
            DropColumn("dbo.GiftCardAssignments", "Days");
        }
    }
}
