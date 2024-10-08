namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeofstatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Status", c => c.String());
            DropColumn("dbo.Appointments", "NoShow");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Appointments", "NoShow", c => c.Boolean(nullable: false));
            DropColumn("dbo.Appointments", "Status");
        }
    }
}
