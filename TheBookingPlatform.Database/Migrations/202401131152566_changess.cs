namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changess : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WaitingLists", "WaitingListStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WaitingLists", "WaitingListStatus");
        }
    }
}
