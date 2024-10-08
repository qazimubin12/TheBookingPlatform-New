namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newslettersettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RebookReminders",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        CustomerName = c.String(),
                        CustomerEmail = c.String(),
                        Date = c.DateTime(nullable: false),
                        Sent = c.Boolean(nullable: false),
                        Opened = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.Appointments", "RebookReminderSent");
            DropColumn("dbo.Appointments", "RebookReminderMailOpened");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Appointments", "RebookReminderMailOpened", c => c.Boolean(nullable: false));
            AddColumn("dbo.Appointments", "RebookReminderSent", c => c.Boolean(nullable: false));
            DropTable("dbo.RebookReminders");
        }
    }
}
