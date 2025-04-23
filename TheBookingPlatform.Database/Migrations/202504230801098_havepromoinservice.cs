namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class havepromoinservice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "HavePromo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "HavePromo");
        }
    }
}
