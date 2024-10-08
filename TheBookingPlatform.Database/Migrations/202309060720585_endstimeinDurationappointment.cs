namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class endstimeinDurationappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "ServiceDuration", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "ServiceDuration");
        }
    }
}
