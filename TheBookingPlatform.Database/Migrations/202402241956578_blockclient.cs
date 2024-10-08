namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class blockclient : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "IsBlocked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "IsBlocked");
        }
    }
}
