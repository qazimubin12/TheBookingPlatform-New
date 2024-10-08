namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class packagesvat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "VAT", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Packages", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Packages", "Price", c => c.Int(nullable: false));
            DropColumn("dbo.Packages", "VAT");
        }
    }
}
