namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class employeedetailsadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Gender", c => c.String());
            AddColumn("dbo.Employees", "PhoneNumber", c => c.String());
            AddColumn("dbo.Employees", "EmailAddress", c => c.String());
            AddColumn("dbo.Employees", "AllowOnlineBooking", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "AllowOnlineBooking");
            DropColumn("dbo.Employees", "EmailAddress");
            DropColumn("dbo.Employees", "PhoneNumber");
            DropColumn("dbo.Employees", "Gender");
        }
    }
}
