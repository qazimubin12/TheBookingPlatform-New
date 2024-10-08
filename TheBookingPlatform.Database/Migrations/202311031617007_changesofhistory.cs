namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofhistory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Histories", "CustomerName", c => c.String());
            AddColumn("dbo.Histories", "EmployeeName", c => c.String());
            DropColumn("dbo.Histories", "CustomerID");
            DropColumn("dbo.Histories", "EmployeeID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Histories", "EmployeeID", c => c.Int(nullable: false));
            AddColumn("dbo.Histories", "CustomerID", c => c.Int(nullable: false));
            DropColumn("dbo.Histories", "EmployeeName");
            DropColumn("dbo.Histories", "CustomerName");
        }
    }
}
