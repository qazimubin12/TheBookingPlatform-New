namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newhcanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "Package", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "Package");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Package", c => c.Int(nullable: false));
            DropColumn("dbo.Companies", "Package");
        }
    }
}
