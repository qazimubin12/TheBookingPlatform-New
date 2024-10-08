namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class employeelinking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "LinkedEmployee", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "LinkedEmployee");
        }
    }
}
