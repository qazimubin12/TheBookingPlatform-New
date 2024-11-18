namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class paymentmessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SumUpTokens", "PaymentMessage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SumUpTokens", "PaymentMessage");
        }
    }
}
