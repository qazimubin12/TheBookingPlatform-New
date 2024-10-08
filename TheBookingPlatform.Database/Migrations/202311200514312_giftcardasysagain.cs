namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardasysagain : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftCards", "Days", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GiftCards", "Days");
        }
    }
}
