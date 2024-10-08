namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class timetablerosters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TimeTableRosters",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        RosterStartDate = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.TimeTables", "RosterStartDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimeTables", "RosterStartDate", c => c.DateTime(nullable: false));
            DropTable("dbo.TimeTableRosters");
        }
    }
}
