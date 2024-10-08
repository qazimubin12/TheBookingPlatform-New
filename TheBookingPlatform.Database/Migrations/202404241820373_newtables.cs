namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newtables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExceptionShifts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RecurringShiftID = c.Int(nullable: false),
                        ExceptionDate = c.DateTime(nullable: false),
                        StartTime = c.String(),
                        EndTime = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.RecurringShifts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ShiftID = c.Int(nullable: false),
                        Frequency = c.String(),
                        RecurEnd = c.String(),
                        RecurEndDate = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Shifts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Day = c.String(),
                        StartTime = c.String(),
                        EndTime = c.String(),
                        IsRecurring = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Shifts");
            DropTable("dbo.RecurringShifts");
            DropTable("dbo.ExceptionShifts");
        }
    }
}
