namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeofdatatype : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CalendarManages", "ManageOf", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CalendarManages", "ManageOf", c => c.Int(nullable: false));
        }
    }
}
