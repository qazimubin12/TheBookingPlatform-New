namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orderingsystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "DisplayOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Services", "DisplayOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "DisplayOrder");
            DropColumn("dbo.Employees", "DisplayOrder");
        }
    }
}
