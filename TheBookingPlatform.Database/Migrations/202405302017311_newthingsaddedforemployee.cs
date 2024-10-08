namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newthingsaddedforemployee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "LimitCalendarHistory", c => c.String());
            AddColumn("dbo.Employees", "Type", c => c.String());
            AddColumn("dbo.Employees", "Percentage", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Percentage");
            DropColumn("dbo.Employees", "Type");
            DropColumn("dbo.Employees", "LimitCalendarHistory");
        }
    }
}
