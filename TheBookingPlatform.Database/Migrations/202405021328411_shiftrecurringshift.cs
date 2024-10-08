namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class shiftrecurringshift : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ExceptionShifts", "RecurringShiftID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExceptionShifts", "RecurringShiftID", c => c.Int(nullable: false));
        }
    }
}
