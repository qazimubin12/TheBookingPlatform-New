namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newsletterweekssettings2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Companies", "NewsletterInterval", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "NewsletterInterval", c => c.String());
        }
    }
}
