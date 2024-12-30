namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notificationsnewo : DbMigration
    {
        public override void Up()
        {
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
        
        public override void Down()
        {
            DropTable("dbo.PushSubscriptionYBPs");
        }
    }
}
