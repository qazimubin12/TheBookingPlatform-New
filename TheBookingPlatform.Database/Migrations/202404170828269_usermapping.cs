namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class usermapping : DbMigration
    {
        public override void Up()
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserMappings");
        }
    }
}
