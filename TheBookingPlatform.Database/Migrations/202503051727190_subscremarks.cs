namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subscremarks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "SubscribtionRemarks", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "SubscribtionRemarks");
        }
    }
}
