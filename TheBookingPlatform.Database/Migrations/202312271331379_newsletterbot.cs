namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newsletterbot : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "HaveNewsLetter", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "HaveNewsLetter");
        }
    }
}
