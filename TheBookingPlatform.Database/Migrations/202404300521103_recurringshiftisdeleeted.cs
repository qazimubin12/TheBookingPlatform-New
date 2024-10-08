namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recurringshiftisdeleeted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExceptionShifts", "ShiftID", c => c.Int(nullable: false));
            AddColumn("dbo.RecurringShifts", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RecurringShifts", "IsDeleted");
            DropColumn("dbo.ExceptionShifts", "ShiftID");
        }
    }
}
