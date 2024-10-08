namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class usercompanymanagement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Company", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Company");
        }
    }
}
