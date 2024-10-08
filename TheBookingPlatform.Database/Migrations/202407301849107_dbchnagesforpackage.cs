namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbchnagesforpackage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "NoOfUsers", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "Features", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Packages", "Features");
            DropColumn("dbo.Packages", "NoOfUsers");
        }
    }
}
