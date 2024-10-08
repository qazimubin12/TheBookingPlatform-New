namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardassignemnt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftCardAssignments", "AssignedAmount", c => c.Single(nullable: false));
            AddColumn("dbo.Invoices", "Remarks", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "Remarks");
            DropColumn("dbo.GiftCardAssignments", "AssignedAmount");
        }
    }
}
