namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class timetablechanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTables", "Type", c => c.String());
            AddColumn("dbo.TimeTables", "Day", c => c.String());
            AddColumn("dbo.TimeTables", "TimeStart", c => c.String());
            AddColumn("dbo.TimeTables", "TimeEnd", c => c.String());
            AddColumn("dbo.TimeTables", "Date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.TimeTables", "StartDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.TimeTables", "Monday");
            DropColumn("dbo.TimeTables", "MondayTime");
            DropColumn("dbo.TimeTables", "MondayType");
            DropColumn("dbo.TimeTables", "Tuesday");
            DropColumn("dbo.TimeTables", "TuesdayTime");
            DropColumn("dbo.TimeTables", "TuesdayType");
            DropColumn("dbo.TimeTables", "Wednesday");
            DropColumn("dbo.TimeTables", "WednesdayTime");
            DropColumn("dbo.TimeTables", "WednesdayType");
            DropColumn("dbo.TimeTables", "Thursday");
            DropColumn("dbo.TimeTables", "ThursdayTime");
            DropColumn("dbo.TimeTables", "ThursdayType");
            DropColumn("dbo.TimeTables", "Friday");
            DropColumn("dbo.TimeTables", "FridayTime");
            DropColumn("dbo.TimeTables", "FridayType");
            DropColumn("dbo.TimeTables", "Saturday");
            DropColumn("dbo.TimeTables", "SaturdayTime");
            DropColumn("dbo.TimeTables", "SaturdayType");
            DropColumn("dbo.TimeTables", "Sunday");
            DropColumn("dbo.TimeTables", "SundayTime");
            DropColumn("dbo.TimeTables", "SundayType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimeTables", "SundayType", c => c.String());
            AddColumn("dbo.TimeTables", "SundayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Sunday", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "SaturdayType", c => c.String());
            AddColumn("dbo.TimeTables", "SaturdayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Saturday", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "FridayType", c => c.String());
            AddColumn("dbo.TimeTables", "FridayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Friday", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "ThursdayType", c => c.String());
            AddColumn("dbo.TimeTables", "ThursdayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Thursday", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "WednesdayType", c => c.String());
            AddColumn("dbo.TimeTables", "WednesdayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Wednesday", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "TuesdayType", c => c.String());
            AddColumn("dbo.TimeTables", "TuesdayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Tuesday", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "MondayType", c => c.String());
            AddColumn("dbo.TimeTables", "MondayTime", c => c.String());
            AddColumn("dbo.TimeTables", "Monday", c => c.Boolean(nullable: false));
            AlterColumn("dbo.TimeTables", "StartDate", c => c.String());
            DropColumn("dbo.TimeTables", "Date");
            DropColumn("dbo.TimeTables", "TimeEnd");
            DropColumn("dbo.TimeTables", "TimeStart");
            DropColumn("dbo.TimeTables", "Day");
            DropColumn("dbo.TimeTables", "Type");
        }
    }
}
