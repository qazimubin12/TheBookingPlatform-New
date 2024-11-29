    namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stripesigninsecret : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "SigningSecret", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "SigningSecret");
        }
    }
}
