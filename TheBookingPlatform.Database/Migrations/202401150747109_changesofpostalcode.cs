namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofpostalcode : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Companies", "PostalCode", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "PostalCode", c => c.Int(nullable: false));
        }
    }
}
