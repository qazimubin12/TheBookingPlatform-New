namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wtihapyamt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "Total", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "Total");
        }
    }
}
