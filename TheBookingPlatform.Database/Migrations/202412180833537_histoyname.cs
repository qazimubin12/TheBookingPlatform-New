namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class histoyname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Histories", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Histories", "Name");
        }
    }
}
