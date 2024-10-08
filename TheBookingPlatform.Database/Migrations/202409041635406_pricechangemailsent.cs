namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pricechangemailsent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PriceChanges", "MailSentSuccess", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PriceChanges", "MailSentSuccess");
        }
    }
}
