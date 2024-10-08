namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isPaidandOnlineDiscount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "IsPaid", c => c.Boolean(nullable: false));
            AddColumn("dbo.Appointments", "OnlinePriceChange", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "OnlinePriceChange");
            DropColumn("dbo.Appointments", "IsPaid");
        }
    }
}
