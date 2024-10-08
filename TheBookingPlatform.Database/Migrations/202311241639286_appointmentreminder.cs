namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class appointmentreminder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Reminder", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "Reminder");
        }
    }
}
