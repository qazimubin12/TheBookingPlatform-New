namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class resourcechanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Resources", "Availability", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Resources", "Availability");
        }
    }
}
