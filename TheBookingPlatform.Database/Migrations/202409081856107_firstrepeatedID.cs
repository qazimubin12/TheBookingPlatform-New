namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class firstrepeatedID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "FirstRepeatedID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "FirstRepeatedID");
        }
    }
}
