namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newfinalchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "FromGCAL", c => c.Boolean(nullable: false));
            AddColumn("dbo.Employees", "WatchChannelID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "WatchChannelID");
            DropColumn("dbo.Appointments", "FromGCAL");
        }
    }
}
