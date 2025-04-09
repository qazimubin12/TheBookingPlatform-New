namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class expeirence : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Experience", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Experience");
        }
    }
}
