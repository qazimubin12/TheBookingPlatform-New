namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class coloronappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Color", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "Color");
        }
    }
}
