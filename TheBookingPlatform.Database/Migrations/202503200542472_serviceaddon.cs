namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class serviceaddon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "AddOn", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "AddOn");
        }
    }
}
