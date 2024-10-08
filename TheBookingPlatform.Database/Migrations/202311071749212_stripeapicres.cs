namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stripeapicres : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "APIKEY", c => c.String());
            AddColumn("dbo.Companies", "PUBLISHEDKEY", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "PUBLISHEDKEY");
            DropColumn("dbo.Companies", "APIKEY");
        }
    }
}
