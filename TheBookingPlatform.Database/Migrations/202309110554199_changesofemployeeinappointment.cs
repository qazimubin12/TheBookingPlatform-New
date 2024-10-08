namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofemployeeinappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "EmployeeID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "EmployeeID");
        }
    }
}
