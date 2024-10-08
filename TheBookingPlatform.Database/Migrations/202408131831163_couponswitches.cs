namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class couponswitches : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CouponSwitches",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CouponID = c.Int(nullable: false),
                        BlastingStatus = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CouponSwitches");
        }
    }
}
