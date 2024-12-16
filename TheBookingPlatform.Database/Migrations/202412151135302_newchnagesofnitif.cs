namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newchnagesofnitif : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "ReadNotifications", c => c.String());
            DropColumn("dbo.Notifications", "ReadByUsers");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "ReadByUsers", c => c.String());
            DropColumn("dbo.AspNetUsers", "ReadNotifications");
            DropColumn("dbo.Notifications", "Date");
        }
    }
}
