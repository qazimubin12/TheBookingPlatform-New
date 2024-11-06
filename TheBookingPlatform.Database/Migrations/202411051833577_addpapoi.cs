namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addpapoi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sales", "AppointmentID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sales", "AppointmentID");
        }
    }
}
