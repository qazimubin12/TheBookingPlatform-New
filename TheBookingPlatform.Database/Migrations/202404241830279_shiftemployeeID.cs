namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class shiftemployeeID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Shifts", "EmployeeID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Shifts", "EmployeeID");
        }
    }
}
