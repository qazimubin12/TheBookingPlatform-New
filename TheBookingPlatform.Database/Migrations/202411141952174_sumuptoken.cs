namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sumuptoken : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SumUpTokens",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Token = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SumUpTokens");
        }
    }
}
