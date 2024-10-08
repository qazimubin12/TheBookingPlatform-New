namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class displayorderadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCategories", "DisplayOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceCategories", "DisplayOrder");
        }
    }
}
