namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class resourceaddition : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "ResourceID", c => c.Int(nullable: false));
            AddColumn("dbo.Resources", "Services", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Resources", "Services");
            DropColumn("dbo.Appointments", "ResourceID");
        }
    }
}
