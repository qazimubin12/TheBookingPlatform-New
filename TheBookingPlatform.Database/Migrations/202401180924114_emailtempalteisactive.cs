namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailtempalteisactive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailTemplates", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailTemplates", "IsActive");
        }
    }
}
