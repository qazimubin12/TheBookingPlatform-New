namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nsomeremoved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Services", c => c.String());
            AddColumn("dbo.Employees", "IsDeleted", c => c.Boolean(nullable: false));
            DropColumn("dbo.Employees", "PhoneNumber");
            DropColumn("dbo.Employees", "EmailAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "EmailAddress", c => c.String());
            AddColumn("dbo.Employees", "PhoneNumber", c => c.String());
            DropColumn("dbo.Employees", "IsDeleted");
            DropColumn("dbo.Employees", "Services");
        }
    }
}
