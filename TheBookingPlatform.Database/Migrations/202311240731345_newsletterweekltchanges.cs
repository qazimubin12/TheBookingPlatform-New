namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newsletterweekltchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "NewsLetterWeekInterval", c => c.Int(nullable: false));
            DropColumn("dbo.Companies", "NewsletterInterval");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "NewsletterInterval", c => c.String());
            DropColumn("dbo.Companies", "NewsLetterWeekInterval");
        }
    }
}
