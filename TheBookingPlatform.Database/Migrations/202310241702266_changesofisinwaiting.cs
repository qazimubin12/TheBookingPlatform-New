namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofisinwaiting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "IsInWaiting", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "IsInWaiting");
        }
    }
}
