namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardexpirydatatypechange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GiftCards", "GiftCardExpiry", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GiftCards", "GiftCardExpiry", c => c.DateTime(nullable: false));
        }
    }
}
