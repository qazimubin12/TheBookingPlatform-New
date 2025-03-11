namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ownercompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "OwnerCompany", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "OwnerCompany");
        }
    }
}
