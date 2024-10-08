namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofapp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "AnyAvailableEmployeeSelected", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "AnyAvailableEmployeeSelected");
        }
    }
}
