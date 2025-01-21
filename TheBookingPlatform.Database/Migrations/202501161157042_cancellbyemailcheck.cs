namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cancellbyemailcheck : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "CancelledByEmail", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "CancelledByEmail");
        }
    }
}
