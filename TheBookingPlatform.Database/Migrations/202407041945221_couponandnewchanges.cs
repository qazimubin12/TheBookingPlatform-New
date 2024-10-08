namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class couponandnewchanges : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CouponAssignments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CouponID = c.Int(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        AssignedDate = c.DateTime(nullable: false),
                        Used = c.Int(nullable: false),
                        IsSentToClient = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Coupons",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        Discount = c.Single(nullable: false),
                        CouponCode = c.String(),
                        UsageCount = c.Int(nullable: false),
                        CouponName = c.String(),
                        CouponDescription = c.String(),
                        IsDisabled = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Appointments", "CouponID", c => c.Int(nullable: false));
            AddColumn("dbo.Appointments", "CouponAssignmentID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "CouponAssignmentID");
            DropColumn("dbo.Appointments", "CouponID");
            DropTable("dbo.Coupons");
            DropTable("dbo.CouponAssignments");
        }
    }
}
