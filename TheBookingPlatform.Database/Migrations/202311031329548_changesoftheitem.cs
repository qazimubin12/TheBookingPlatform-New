namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesoftheitem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailTemplates", "Duration", c => c.String());
            DropColumn("dbo.EmailTemplates", "ScheduleDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EmailTemplates", "ScheduleDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.EmailTemplates", "Duration");
        }
    }
}
