namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nonselectedemployee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WaitingLists", "NonSelectedEmployee", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WaitingLists", "NonSelectedEmployee");
        }
    }
}
