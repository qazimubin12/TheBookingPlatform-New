namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class historiesinappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Histories", "AppointmentID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Histories", "AppointmentID");
        }
    }
}
