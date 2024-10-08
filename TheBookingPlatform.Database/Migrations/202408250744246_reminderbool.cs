namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reminderbool : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reminders", "AppointmentID", c => c.Int(nullable: false));
            AddColumn("dbo.Reminders", "Sent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reminders", "Sent");
            DropColumn("dbo.Reminders", "AppointmentID");
        }
    }
}
