namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cardassignmentchanges : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.GiftCardAssignments", "FirstName");
            DropColumn("dbo.GiftCardAssignments", "LastName");
            DropColumn("dbo.GiftCardAssignments", "Email");
            DropColumn("dbo.GiftCardAssignments", "MobileNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GiftCardAssignments", "MobileNumber", c => c.String());
            AddColumn("dbo.GiftCardAssignments", "Email", c => c.String());
            AddColumn("dbo.GiftCardAssignments", "LastName", c => c.String());
            AddColumn("dbo.GiftCardAssignments", "FirstName", c => c.String());
        }
    }
}
