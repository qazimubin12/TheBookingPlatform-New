namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noneneded : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Companies", "SigningSecret");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "SigningSecret", c => c.String());
        }
    }
}
