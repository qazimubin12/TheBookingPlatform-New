namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FranchiseRequests", "MappedToUserID", c => c.String());
            DropTable("dbo.UserMappings");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserMappings",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserIDTo = c.String(),
                        UserIDFrom = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.FranchiseRequests", "MappedToUserID");
        }
    }
}
