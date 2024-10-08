namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pricechange : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PriceChanges",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TypeOfChange = c.String(),
                        Percentage = c.Single(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Repeat = c.Boolean(nullable: false),
                        Frequency = c.String(),
                        Every = c.String(),
                        Days = c.String(),
                        Ends = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PriceChanges");
        }
    }
}
