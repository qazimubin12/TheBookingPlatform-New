namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class intervaloncalendar : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IntervalCalendar", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IntervalCalendar");
        }
    }
}
