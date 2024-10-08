namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class serviceIsActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "IsActive");
        }
    }
}
