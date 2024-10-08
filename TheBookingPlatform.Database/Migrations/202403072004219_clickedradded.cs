namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class clickedradded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RebookReminders", "Clicked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RebookReminders", "Clicked");
        }
    }
}
