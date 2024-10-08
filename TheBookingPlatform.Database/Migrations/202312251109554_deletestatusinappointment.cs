namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deletestatusinappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "DELETED", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "DELETED");
        }
    }
}
