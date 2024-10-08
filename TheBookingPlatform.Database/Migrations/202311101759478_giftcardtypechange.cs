namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardtypechange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Histories", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Histories", "Type");
        }
    }
}
