namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class endstimeinappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "EndTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "EndTime");
        }
    }
}
