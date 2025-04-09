namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class experiencyear : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "ExpYears", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "ExpYears");
        }
    }
}
