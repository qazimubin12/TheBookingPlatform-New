namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loyaltycardassignmentdays : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LoyaltyCardAssignments", "Days", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LoyaltyCardAssignments", "Days");
        }
    }
}
