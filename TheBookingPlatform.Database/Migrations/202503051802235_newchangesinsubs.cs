namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newchangesinsubs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "SubscriptionID", c => c.String());
            AddColumn("dbo.Companies", "SubscriptionStatus", c => c.String());
            DropColumn("dbo.AspNetUsers", "SubscribtionRemarks");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "SubscribtionRemarks", c => c.String());
            DropColumn("dbo.Companies", "SubscriptionStatus");
            DropColumn("dbo.Companies", "SubscriptionID");
        }
    }
}
