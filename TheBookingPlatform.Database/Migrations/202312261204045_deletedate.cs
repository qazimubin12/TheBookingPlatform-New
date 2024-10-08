namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deletedate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "DeletedTime", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "DeletedTime");
        }
    }
}
