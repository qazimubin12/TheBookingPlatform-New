namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class franchiserequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FranchiseRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.String(),
                        Accepted = c.Boolean(nullable: false),
                        Status = c.String(),
                        CompanyIDFrom = c.Int(nullable: false),
                        CompanyIDFor = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FranchiseRequests");
        }
    }
}
