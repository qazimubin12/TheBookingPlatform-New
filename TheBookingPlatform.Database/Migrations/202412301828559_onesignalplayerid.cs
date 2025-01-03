namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class onesignalplayerid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PlayerID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PlayerID");
        }
    }
}
