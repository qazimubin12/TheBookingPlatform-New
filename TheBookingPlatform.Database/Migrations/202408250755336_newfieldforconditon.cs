namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newfieldforconditon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reminders", "Paid", c => c.Boolean(nullable: false));
            AddColumn("dbo.Reminders", "IsCancelled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Reminders", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reminders", "Deleted");
            DropColumn("dbo.Reminders", "IsCancelled");
            DropColumn("dbo.Reminders", "Paid");
        }
    }
}
