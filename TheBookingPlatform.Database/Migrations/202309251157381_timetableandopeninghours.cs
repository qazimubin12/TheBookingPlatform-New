namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class timetableandopeninghours : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OpeningHours",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Day = c.String(),
                        Time = c.String(),
                        isClosed = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.TimeTables",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        StartDate = c.String(),
                        Monday = c.Boolean(nullable: false),
                        MondayTime = c.String(),
                        MondayType = c.String(),
                        Tuesday = c.Boolean(nullable: false),
                        TuesdayTime = c.String(),
                        TuesdayType = c.String(),
                        Wednesday = c.Boolean(nullable: false),
                        WednesdayTime = c.String(),
                        WednesdayType = c.String(),
                        Thursday = c.Boolean(nullable: false),
                        ThursdayTime = c.String(),
                        ThursdayType = c.String(),
                        Friday = c.Boolean(nullable: false),
                        FridayTime = c.String(),
                        FridayType = c.String(),
                        Saturday = c.Boolean(nullable: false),
                        SaturdayTime = c.String(),
                        SaturdayType = c.String(),
                        Sunday = c.Boolean(nullable: false),
                        SundayTime = c.String(),
                        SundayType = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TimeTables");
            DropTable("dbo.OpeningHours");
        }
    }
}
