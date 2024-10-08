namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class giftcardandassignemtnsandemailtemplatescheduledate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GiftCardAssignments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        GiftCardID = c.Int(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        AssignedDate = c.DateTime(nullable: false),
                        Balance = c.Single(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        MobileNumber = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.GiftCards",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Code = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        Date = c.DateTime(nullable: false),
                        GiftCardExpiry = c.DateTime(nullable: false),
                        HaveExpiry = c.Boolean(nullable: false),
                        GiftCardAmount = c.String(),
                        GiftCardImage = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.EmailTemplates", "ScheduleDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailTemplates", "ScheduleDate");
            DropTable("dbo.GiftCards");
            DropTable("dbo.GiftCardAssignments");
        }
    }
}
