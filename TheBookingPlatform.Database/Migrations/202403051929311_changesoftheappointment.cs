namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesoftheappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "RebookReminderMailOpened", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "RebookReminderMailOpened");
        }
    }
}
