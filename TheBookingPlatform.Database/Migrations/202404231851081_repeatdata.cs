namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class repeatdata : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTables", "Repeat", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTables", "RepeatEnd", c => c.String());
            AddColumn("dbo.TimeTables", "DateOfRepeatEnd", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTables", "DateOfRepeatEnd");
            DropColumn("dbo.TimeTables", "RepeatEnd");
            DropColumn("dbo.TimeTables", "Repeat");
        }
    }
}
