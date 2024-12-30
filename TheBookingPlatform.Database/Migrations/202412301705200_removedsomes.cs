namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removedsomes : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.PushSubscriptionYBPs");
            DropTable("dbo.SumUpTokens");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SumUpTokens",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Token = c.String(),
                        PaymentMessage = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.PushSubscriptionYBPs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Endpoint = c.String(),
                        P256DH = c.String(),
                        AuthSecret = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
