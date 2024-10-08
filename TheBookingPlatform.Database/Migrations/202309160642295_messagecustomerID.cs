namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class messagecustomerID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "CustomerID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "CustomerID");
        }
    }
}
