namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notificationchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Code", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Code");
        }
    }
}
