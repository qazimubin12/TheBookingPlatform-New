namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class employeeIds : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WaitingLists", "EmployeeIDs", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WaitingLists", "EmployeeIDs");
        }
    }
}
