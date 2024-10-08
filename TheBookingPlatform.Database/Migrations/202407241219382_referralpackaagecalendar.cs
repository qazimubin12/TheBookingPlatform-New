namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class referralpackaagecalendar : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Packages",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        APIKEY = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        Price = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.String(),
                        PackageID = c.Int(nullable: false),
                        LastPaidDate = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Referrals",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ReferralCode = c.String(),
                        ReferredBy = c.Int(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        GrandTotal = c.Single(nullable: false),
                        AppointmentID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Customers", "ReferralCode", c => c.String());
            AddColumn("dbo.Customers", "ReferralBalance", c => c.Single(nullable: false));
            AddColumn("dbo.AspNetUsers", "IsInTrialPeriod", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "IsPaid", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "Package", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "LastPaymentDate", c => c.String());
            AddColumn("dbo.AspNetUsers", "RegisteredDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "RegisteredDate");
            DropColumn("dbo.AspNetUsers", "LastPaymentDate");
            DropColumn("dbo.AspNetUsers", "Package");
            DropColumn("dbo.AspNetUsers", "IsPaid");
            DropColumn("dbo.AspNetUsers", "IsInTrialPeriod");
            DropColumn("dbo.Customers", "ReferralBalance");
            DropColumn("dbo.Customers", "ReferralCode");
            DropTable("dbo.Referrals");
            DropTable("dbo.Payments");
            DropTable("dbo.Packages");
        }
    }
}
